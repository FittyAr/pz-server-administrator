using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using pz_server_administrator.Services;

namespace pz_server_administrator.BackgroundServices;

/// <summary>
/// Servicio en segundo plano que vigila el `server-console.txt` (o similar)
/// para detectar crashes en tiempo real y disparar a la IA si está configurada en modo autónomo.
/// </summary>
public class PzLogObserver : BackgroundService
{
    private readonly ILogger<PzLogObserver> _logger;
    private readonly IConfigurationService _configService;
    // Utilizamos IServiceProvider para resolver IAiService ya que BackgroundService es Singleton y IAiService puede tener dependencias Scoped.
    private readonly IServiceProvider _serviceProvider;

    public PzLogObserver(
        ILogger<PzLogObserver> logger,
        IConfigurationService configService,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configService = configService;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[LogObserver] Iniciando monitoreo de logs del servidor Project Zomboid...");

        while (!stoppingToken.IsCancellationRequested)
        {
            var config = _configService.GetConfiguration();
            var serverDir = config.AppSettings.ServerDirectoryPath;
            var activeServer = config.AppSettings.ActiveServer;

            if (string.IsNullOrEmpty(serverDir) || string.IsNullOrEmpty(activeServer))
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                continue;
            }

            // En un entorno de servidor dedicado, PZ suele escupir los logs en Console o en Zomboid/Logs
            // Buscamos el log más reciente del servidor activo (usualmente Zomboid/Logs/YYYY-MM-DD_HH-mm-ss_ServerConsole.txt)
            // Para propósitos del MVP de IA, monitorizaremos si existe algún server-console.txt base o buscaremos las excepciones recientes.

            // Asumimos que la carpeta Zomboid/Logs está en el root del usuario
            var logsPattern = "*ServerConsole.txt";
            var rootDir = new DirectoryInfo(serverDir).Parent?.FullName;
            if (rootDir != null)
            {
                var logsPath = Path.Combine(rootDir, "Logs");

                if (Directory.Exists(logsPath))
                {
                    // Vigilar cambios (Simplificación para detectar excepciones en el log)
                    // Podríamos implementar un FileSystemWatcher o leer el final del archivo cada x segundos.
                    await CheckLatestLogForErrorsAsync(logsPath, logsPattern, stoppingToken);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task CheckLatestLogForErrorsAsync(string logsPath, string logsPattern, CancellationToken cancellationToken)
    {
        try
        {
            var files = Directory.GetFiles(logsPath, logsPattern, SearchOption.TopDirectoryOnly);
            if (files.Length == 0) return;

            // Obtener el más reciente
            var latestLog = files.OrderByDescending(f => File.GetLastWriteTimeUtc(f)).First();

            // Leer las últimas líneas
            using var stream = new FileStream(latestLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);

            // Posicionarse cerca del final (lectura rápida de los últimos ~2000 bytes)
            if (stream.Length > 2000)
                stream.Seek(-2000, SeekOrigin.End);

            var content = await reader.ReadToEndAsync(cancellationToken);

            // Búsqueda heurística simple de errores
            if (content.Contains("Exception") || content.Contains("StackOverflow") || content.Contains("Fatal") || content.Contains("java.lang."))
            {
                // Un error de mod fue detectado
                _logger.LogWarning("[LogObserver] Se ha detectado una excepción en el log activo: {File}", Path.GetFileName(latestLog));

                // Extraer el snippet del error (últimas 20 líneas)
                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var snippet = string.Join("\n", lines.TakeLast(20));

                // Aquí emitiríamos un evento o guardaríamos el estado para que la UI lo recoja
                // TODO: Integrar con IAiService si AutoFix está activado
            }
        }
        catch (Exception ex)
        {
            _logger.LogTrace("[LogObserver] Error leyendo logs: {Msg}", ex.Message);
        }
    }
}

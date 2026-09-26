using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using pz_server_administrator.Data.Database.Players;
using pz_server_administrator.Data.Database.Vehicles;
using pz_server_administrator.Data.Database.ServerTest;
using pz_server_administrator.Data.Database.Mods;
using System.IO;

namespace pz_server_administrator.Services;

public class DatabaseContextFactory : IDatabaseContextFactory
{
    private readonly IConfigurationService _configurationService;
    private readonly IPzServerService _pzServerService;
    private readonly ILogger<DatabaseContextFactory> _logger;

    public DatabaseContextFactory(
        IConfigurationService configurationService, 
        IPzServerService pzServerService,
        ILogger<DatabaseContextFactory> logger)
    {
        _configurationService = configurationService;
        _pzServerService = pzServerService;
        _logger = logger;
    }

    private string? GetServerDirectory()
    {
        var config = _configurationService.GetConfiguration();
        return config?.AppSettings?.ServerDirectoryPath;
    }

    public PlayersContext? CreatePlayersContext()
    {
        try
        {
            var path = _configurationService.GetConfiguration()?.AppSettings?.PlayersDatabasePath;
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;

            var options = new DbContextOptionsBuilder<PlayersContext>()
                .UseSqlite($"Data Source={path}")
                .Options;

            return new PlayersContext(options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DatabaseContextFactory] Error al inicializar PlayersContext");
            return null;
        }
    }

    public VehiclesContext? CreateVehiclesContext()
    {
        try
        {
            var path = _configurationService.GetConfiguration()?.AppSettings?.VehiclesDatabasePath;
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;

            var options = new DbContextOptionsBuilder<VehiclesContext>()
                .UseSqlite($"Data Source={path}")
                .Options;

            return new VehiclesContext(options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DatabaseContextFactory] Error al inicializar VehiclesContext");
            return null;
        }
    }

    public ServerTestContext? CreateServerTestContext()
    {
        try
        {
            var path = _configurationService.GetConfiguration()?.AppSettings?.ServerTestDatabasePath;
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;

            var options = new DbContextOptionsBuilder<ServerTestContext>()
                .UseSqlite($"Data Source={path}")
                .Options;

            return new ServerTestContext(options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DatabaseContextFactory] Error al inicializar ServerTestContext");
            return null;
        }
    }

    public ModsContext? CreateModsContext()
    {
        try
        {
            var path = _configurationService.GetConfiguration()?.AppSettings?.ModsDatabasePath;

            // Si no está configurado, usamos uno por defecto en el directorio de configuración persistente
            if (string.IsNullOrEmpty(path))
            {
                if (Directory.Exists("/app/config"))
                {
                    path = "/app/config/Mods.db";
                }
                else
                {
                    path = "Mods.db";
                }
            }

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var options = new DbContextOptionsBuilder<ModsContext>()
                .UseSqlite($"Data Source={path}")
                .Options;

            var context = new ModsContext(options);

            // Aseguramos que la DB y el esquema existan (archivo propio de la app)
            context.Database.EnsureCreated();

            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DatabaseContextFactory] Error al inicializar ModsContext");
            return null;
        }
    }
}

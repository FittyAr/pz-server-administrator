using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using pz_server_administrator.Data.Database.Mods;

namespace pz_server_administrator.Services;

/// <summary>
/// Implementación del motor de descubrimiento y sincronización de mods.
/// </summary>
public class ModDiscoveryService : IModDiscoveryService
{
    private readonly ILogger<ModDiscoveryService> _logger;
    private readonly IDatabaseContextFactory _contextFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfigurationService _configurationService;

    public ModDiscoveryService(
        ILogger<ModDiscoveryService> logger,
        IDatabaseContextFactory contextFactory,
        IHttpClientFactory httpClientFactory,
        IConfigurationService configurationService)
    {
        _logger = logger;
        _contextFactory = contextFactory;
        _httpClientFactory = httpClientFactory;
        _configurationService = configurationService;
    }

    /// <summary>
    /// Escanea las carpetas de mods locales y sincroniza con la base de datos Mods.db.
    /// </summary>
    /// <param name="workshopPath">La ruta base donde se encuentran los mods de Steam Workshop.</param>
    public async Task DiscoverLocalModsAsync(string workshopPath)
    {
        if (string.IsNullOrEmpty(workshopPath) || !Directory.Exists(workshopPath))
        {
            _logger.LogWarning("[ModDiscovery] Ruta de workshop inválida: {Path}", workshopPath);
            return;
        }

        // Obtener mods activos desde la configuración del servidor
        var activeModIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var workshopOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var modOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        var appConfig = _configurationService.GetConfiguration();
        var iniPath = appConfig.AppSettings.IniFilePath;
        if (File.Exists(iniPath))
        {
            var content = File.ReadAllText(iniPath);

            // Extraer Workshops y su orden
            var wsMatch = Regex.Match(content, @"^WorkshopItems=([^\r\n]*)", RegexOptions.Multiline);
            if (wsMatch.Success)
            {
                var ids = wsMatch.Groups[1].Value.Split(';');
                for (int i = 0; i < ids.Length; i++)
                {
                    var id = ids[i].Trim();
                    if (!string.IsNullOrEmpty(id)) workshopOrder[id] = i;
                }
            }

            // Extraer Mods activos y su orden
            var mMatch = Regex.Match(content, @"^Mods=([^\r\n]*)", RegexOptions.Multiline);
            if (mMatch.Success)
            {
                var mods = mMatch.Groups[1].Value.Split(';');
                for (int i = 0; i < mods.Length; i++)
                {
                    var m = mods[i].Trim();
                    if (!string.IsNullOrEmpty(m))
                    {
                        activeModIds.Add(m);
                        modOrder[m] = i;
                    }
                }
            }
        }

        // Project Zomboid usa el AppID 108600
        var contentPath = Path.Combine(workshopPath, "content", "108600");

        // Redundancia: Si no existe la subcapitulo content/108600, intentamos escanear la raíz directamente
        // por si el usuario apuntó directamente a la carpeta de contenido.
        var scanRoot = Directory.Exists(contentPath) ? contentPath : workshopPath;

        _logger.LogInformation("[ModDiscovery] Iniciando escaneo en: {Path}", scanRoot);

        using var context = _contextFactory.CreateModsContext();
        if (context == null) return;

        // Buscamos todos los mod.info de forma recursiva desde la raíz de escaneo
        var allModInfoFiles = Directory.GetFiles(scanRoot, "mod.info", SearchOption.AllDirectories);
        _logger.LogInformation("[ModDiscovery] Se encontraron {Count} archivos mod.info", allModInfoFiles.Length);

        // Agrupamos por carpeta de nivel superior (que usualmente es el WorkshopId)
        // Estructura esperada: .../ScanRoot/{WorkshopId}/.../mod.info
        foreach (var modInfoPath in allModInfoFiles)
        {
            var relativePath = Path.GetRelativePath(scanRoot, modInfoPath);
            var pathParts = relativePath.Split(Path.DirectorySeparatorChar);

            // El primer segmento de la ruta relativa debería ser el ID del Workshop (ej: 123456789)
            var workshopId = pathParts[0];

            // Si el nombre de la carpeta no parece un ID numérico largo, intentamos usar el nombre de la carpeta superior
            if (!long.TryParse(workshopId, out _) && pathParts.Length > 1)
            {
                // Fallback: usar el nombre de la carpeta que contiene mod.info si no hay ID numérico
                workshopId = Path.GetFileName(Path.GetDirectoryName(modInfoPath)) ?? "Unknown";
            }

            var item = await context.WorkshopItems
                .Include(i => i.Instances)
                .FirstOrDefaultAsync(i => i.Id == workshopId);

            if (item == null)
            {
                item = new WorkshopItem
                {
                    Id = workshopId,
                    Title = $"Mod {workshopId}",
                    Instances = new List<ModInstance>(),
                    Order = workshopOrder.TryGetValue(workshopId, out var o) ? o : 999
                };
                context.WorkshopItems.Add(item);
            }
            else
            {
                item.Order = workshopOrder.TryGetValue(workshopId, out var o) ? o : 999;
            }

            var modData = ParseModInfo(modInfoPath);
            if (modData == null) continue;

            var modId = modData["id"];
            var modName = modData["name"];

            var instance = item.Instances.FirstOrDefault(i => i.ModId == modId);
            if (instance == null)
            {
                instance = new ModInstance
                {
                    ModId = modId,
                    Name = modName,
                    WorkshopItemId = workshopId,
                    IsActive = activeModIds.Contains(modId),
                    Order = modOrder.TryGetValue(modId, out var mo) ? mo : 999,
                    Category = CategorizeMod(modId, modName, modData)
                };
                item.Instances.Add(instance);
            }
            else
            {
                instance.Name = modName;
                instance.IsActive = activeModIds.Contains(modId);
                instance.Order = modOrder.TryGetValue(modId, out var mo) ? mo : 999;
                instance.Category = CategorizeMod(modId, modName, modData);
            }
        }

        try
        {
            await context.SaveChangesAsync();
            _logger.LogInformation("[ModDiscovery] Sincronización local completada.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ModDiscovery] Error al guardar en la base de datos.");
        }
    }

    public async Task<List<WorkshopItem>> GetWorkshopItemsAsync()
    {
        using var context = _contextFactory.CreateModsContext();
        if (context == null) return new List<WorkshopItem>();

        return await context.WorkshopItems
            .Include(i => i.Instances)
            .OrderBy(i => i.Order)
            .ThenBy(i => i.Title)
            .ToListAsync();
    }

    /// <summary>
    /// Busca metadatos en Steam Workshop para un artículo específico.
    /// </summary>
    public async Task<bool> FetchSteamMetadataAsync(string workshopId)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

        try
        {
            var url = $"https://steamcommunity.com/sharedfiles/filedetails/?id={workshopId}";
            var response = await client.GetStringAsync(url);

            // Regex básico para extraer metadatos (evitamos dependencias pesadas de parsing HTML por ahora)
            var titleMatch = Regex.Match(response, @"<div class=""workshopItemTitle"">([^<]+)</div>");
            var imageMatch = Regex.Match(response, @"<img id=""previewImageMain"".*?src=""([^""]+)""");

            if (titleMatch.Success)
            {
                using var context = _contextFactory.CreateModsContext();
                var item = await context!.WorkshopItems.FindAsync(workshopId);
                if (item != null)
                {
                    item.Title = titleMatch.Groups[1].Value.Trim();
                    if (imageMatch.Success)
                    {
                        item.ThumbnailPath = imageMatch.Groups[1].Value;
                    }
                    item.LastUpdated = DateTime.Now;

                    await context.SaveChangesAsync();
                    _logger.LogInformation("[ModDiscovery] Metadatos actualizados para {Id}: {Title}", workshopId, item.Title);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ModDiscovery] Error al obtener metadatos de Steam para {Id}", workshopId);
        }
        return false;
    }

    public async Task ToggleModActiveAsync(string workshopId, string modId, bool active)
    {
        using var context = _contextFactory.CreateModsContext();
        if (context == null) return;

        var instance = await context.ModInstances
            .FirstOrDefaultAsync(i => i.WorkshopItemId == workshopId && i.ModId == modId);

        if (instance != null)
        {
            instance.IsActive = active;
            await context.SaveChangesAsync();
        }
    }

    public async Task SaveModConfigurationAsync()
    {
        var appConfig = _configurationService.GetConfiguration();
        var iniPath = appConfig.AppSettings.IniFilePath;
        if (!File.Exists(iniPath)) return;

        using var context = _contextFactory.CreateModsContext();
        if (context == null) return;

        // Recuperar items ordenados. Los que estén activos deben aparecer primero en su respectivo bloque si lo deseamos, 
        // pero PZ requiere que TODOS los WorkshopItems estén en la lista si se van a descargar.
        var workshopIds = await context.WorkshopItems
            .OrderBy(i => i.Order)
            .Select(i => i.Id)
            .ToListAsync();

        // Recuperar IDs de mods activos ordenados
        var activeModIds = await context.ModInstances
            .Where(i => i.IsActive)
            .OrderBy(i => i.Order)
            .Select(i => i.ModId)
            .ToListAsync();

        var content = await File.ReadAllTextAsync(iniPath);

        // Regx para WorkshopItems=
        content = Regex.Replace(content, @"^WorkshopItems=[^\r\n]*", $"WorkshopItems={string.Join(";", workshopIds)}", RegexOptions.Multiline);

        // Regex para Mods=
        content = Regex.Replace(content, @"^Mods=[^\r\n]*", $"Mods={string.Join(";", activeModIds)}", RegexOptions.Multiline);

        await File.WriteAllTextAsync(iniPath, content);
        _logger.LogInformation("[ModDiscovery] Configuración guardada en .ini: {WorkshopCount} workshops, {ModCount} mods activos.", workshopIds.Count, activeModIds.Count);
    }

    private Dictionary<string, string>? ParseModInfo(string path)
    {
        try
        {
            var lines = File.ReadAllLines(path);
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("--")) continue;

                var parts = trimmed.Split('=', 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim().ToLower();
                    var val = parts[1].Trim();

                    if (result.ContainsKey(key))
                        result[key] += ";" + val; // Manejar múltiples líneas de mapas o requerimientos
                    else
                        result[key] = val;
                }
            }

            if (!result.ContainsKey("id") || !result.ContainsKey("name"))
                return null;

            return result;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Intenta categorizar el mod basado en su ID, nombre y contenido de mod.info.
    /// </summary>
    private ModCategory CategorizeMod(string id, string name, Dictionary<string, string> data)
    {
        id = id.ToLower();
        name = name.ToLower();

        // Maps: Si tiene la entrada 'map'
        if (data.ContainsKey("map")) return ModCategory.Maps;

        // Localization: Si contiene palabras clave
        if (id.Contains("localization") || id.Contains("translate") || name.Contains("traduccion") || name.Contains("spanish") || name.Contains("english"))
            return ModCategory.Localization;

        // Vehicles: Entrada en tags o palabras clave en el ID
        if (id.Contains("car") || id.Contains("vehicle") || id.Contains("truck") || name.Contains("car") || name.Contains("vehicle"))
            return ModCategory.Vehicles;

        // Framework: Whitelist básica
        var frameworks = new[] { "framework", "library", "lib", "api", "hook", "moodles" };
        if (frameworks.Any(f => id.Contains(f) || name.Contains(f)))
            return ModCategory.Framework;

        // Clothing/UI
        if (id.Contains("clothing") || id.Contains("skin") || id.Contains("interface") || id.Contains("ui"))
            return ModCategory.ClothingInterface;

        return ModCategory.Other;
    }
}

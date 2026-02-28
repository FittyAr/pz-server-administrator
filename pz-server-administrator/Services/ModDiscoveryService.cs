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

            // Calculamos hash para detectar cambios (Hacemos un hash básico del contenido de mod.info y la fecha)
            item.VersionHash = CalculateLocalHash(modInfoPath);
            item.LocalUpdatedAt = File.GetLastWriteTimeUtc(modInfoPath);

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

            // Fase 3: Sincronizar metadatos desde la Web (Scraping)
            await SyncWorkshopMetadataAsync(context);
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

    public async Task<CloudProfile> GetCloudProfileAsync()
    {
        using var context = _contextFactory.CreateModsContext();
        if (context == null) return new CloudProfile();

        var profile = await context.CloudProfiles.FirstOrDefaultAsync(p => p.Id == 1);
        if (profile == null)
        {
            profile = new CloudProfile { Id = 1, ApiKey = "" };
            context.CloudProfiles.Add(profile);
            await context.SaveChangesAsync();
        }
        return profile;
    }

    public async Task UpdateCloudProfileAsync(CloudProfile profile)
    {
        using var context = _contextFactory.CreateModsContext();
        if (context == null) return;

        var existing = await context.CloudProfiles.FirstOrDefaultAsync(p => p.Id == 1);
        if (existing == null)
        {
            context.CloudProfiles.Add(profile);
        }
        else
        {
            existing.ApiKey = profile.ApiKey;
            existing.AutoReport = profile.AutoReport;
            existing.CloudSyncEnabled = profile.CloudSyncEnabled;
            existing.LastSync = profile.LastSync;
        }

        await context.SaveChangesAsync();
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

    private string CalculateLocalHash(string modInfoPath)
    {
        try
        {
            var fileInfo = new FileInfo(modInfoPath);
            var content = File.ReadAllText(modInfoPath);
            // Hash simple combinando contenido y fecha de modificación
            var rawData = $"{content}|{fileInfo.LastWriteTimeUtc.Ticks}|{fileInfo.Length}";
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(rawData);
            var hashBytes = sha.ComputeHash(bytes);
            return Convert.ToHexString(hashBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ModDiscovery] Error calculando hash para {Path}", modInfoPath);
            return "ERROR";
        }
    }

    private async Task SyncWorkshopMetadataAsync(ModsContext context)
    {
        _logger.LogInformation("[ModDiscovery] Iniciando sincronización de metadatos desde Steam Workshop...");
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(10);

        var itemsToSync = await context.WorkshopItems
            .Where(i => string.IsNullOrEmpty(i.ThumbnailPath) || i.Title.StartsWith("Mod ") || i.Description == null)
            .ToListAsync();

        foreach (var item in itemsToSync)
        {
            try
            {
                var id = item.Id;
                if (!long.TryParse(id, out _)) continue; // No es un ID de Steam real

                var steamUrl = $"https://steamcommunity.com/sharedfiles/filedetails/?id={id}";
                var html = await client.GetStringAsync(steamUrl);

                // Extracción de título
                var titleMatch = Regex.Match(html, @"<div class=""workshopItemTitle"">(.+?)</div>", RegexOptions.IgnoreCase);
                if (titleMatch.Success)
                    item.Title = System.Net.WebUtility.HtmlDecode(titleMatch.Groups[1].Value);

                // Extracción de thumbnail
                var thumbMatch = Regex.Match(html, @"id=""previewImageMain"".*?src=""(.+?)""", RegexOptions.IgnoreCase);
                if (thumbMatch.Success)
                    item.ThumbnailPath = thumbMatch.Groups[1].Value;

                // Extracción de descripción
                var descMatch = Regex.Match(html, @"<div id=""workshopItemDescription"" class=""workshopItemDescription"">(.+?)</div>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (descMatch.Success)
                {
                    var cleanDesc = Regex.Replace(descMatch.Groups[1].Value, "<.*?>", string.Empty);
                    item.Description = System.Net.WebUtility.HtmlDecode(cleanDesc).Trim();
                    if (item.Description.Length > 800) item.Description = item.Description.Substring(0, 797) + "...";
                }

                // Extracción de fecha de actualización (ej: "Actualizado: 12 oct. 2023 a las 14:10")
                var dateMatch = Regex.Match(html, @"<div class=""detailsStatRight"">(.+?)</div>", RegexOptions.IgnoreCase);
                if (dateMatch.Success)
                {
                    // Nota: Steam usa formatos muy variados según el idioma. 
                    // Para simplificar, si detectamos un cambio en el texto de la fecha respecto al guardado, asumimos cambio.
                    var dateText = dateMatch.Groups[1].Value.Trim();
                    // Intentamos parsear si es posible, si no, guardamos el hash del texto para comparar.
                    if (DateTime.TryParse(dateText, out var parsedDate))
                    {
                        item.LastUpdated = parsedDate;

                        // Si la fecha de Steam es posterior a la local, marcamos como desactualizado
                        if (item.LocalUpdatedAt.HasValue && item.LastUpdated.Value > item.LocalUpdatedAt.Value.AddMinutes(5))
                        {
                            item.IsUpdateAvailable = true;
                        }
                        else
                        {
                            item.IsUpdateAvailable = false;
                        }
                    }
                }

                await context.SaveChangesAsync();
                _logger.LogInformation("[ModDiscovery] Metadatos actualizados para {Id}.", id);
                await Task.Delay(300); // Pequeño retraso para cortesía
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[ModDiscovery] Fallo parcial de metadatos para {Id}: {Msg}", item.Id, ex.Message);
            }
        }
    }

    public async Task TriggerModUpdateAsync(string workshopId)
    {
        _logger.LogInformation("[ModDiscovery] Se ha solicitado una actualización forzosa para el mod {Id} vía SteamCMD.", workshopId);
        // Aquí se podría guardar un flag en un archivo que el entrypoint del contenedor revise para forzar update
        // O ejecutar un comando shell si tenemos permisos:
        // Process.Start("steamcmd", $"+login anonymous +workshop_download_item 108600 {workshopId} +quit");
        await Task.Delay(500);
    }

    public async Task BatchApplyAiActionsAsync(IEnumerable<AiAction> actions)
    {
        using var context = _contextFactory.CreateModsContext();
        if (context == null) return;
        var allMods = await context.ModInstances.Include(m => m.WorkshopItem).ToListAsync();

        foreach (var action in actions)
        {
            _logger.LogInformation("[AI Agent] Ejecutando acción: {Type} sobre {Target}", action.Type, action.TargetId);

            switch (action.Type)
            {
                case AiActionType.Deactivate:
                    var toDeactivate = allMods.FirstOrDefault(m => m.ModId == action.TargetId);
                    if (toDeactivate != null) toDeactivate.IsActive = false;
                    break;

                case AiActionType.Activate:
                    var toActivate = allMods.FirstOrDefault(m => m.ModId == action.TargetId);
                    if (toActivate != null) toActivate.IsActive = true;
                    break;

                case AiActionType.Reorder:
                    var toReorder = allMods.FirstOrDefault(m => m.ModId == action.TargetId);
                    if (toReorder != null && action.Parameters.TryGetValue("new_order", out var orderStrValue) && int.TryParse(orderStrValue, out var newOrderInteger))
                    {
                        toReorder.Order = newOrderInteger;
                    }
                    break;
            }
        }

        await context.SaveChangesAsync();
        await SaveModConfigurationAsync(); // Persistir a archivos .ini / .lua
    }
}

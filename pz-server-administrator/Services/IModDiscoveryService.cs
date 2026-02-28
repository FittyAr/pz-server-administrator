using pz_server_administrator.Data.Database.Mods;

namespace pz_server_administrator.Services;

/// <summary>
/// Interfaz para el servicio de descubrimiento y escaneo de mods.
/// </summary>
public interface IModDiscoveryService
{
    /// <summary>
    /// Escanea las carpetas locales de mods y sincroniza con la base de datos Mods.db.
    /// </summary>
    /// <param name="workshopPath">Ruta a la carpeta 'Workshop' de Project Zomboid.</param>
    /// <returns>Tarea completada.</returns>
    Task DiscoverLocalModsAsync(string workshopPath);

    /// <summary>
    /// Obtiene todos los artículos del workshop de la base de datos local.
    /// </summary>
    /// <returns>Lista de WorkshopItems.</returns>
    Task<List<WorkshopItem>> GetWorkshopItemsAsync();

    /// <summary>
    /// Busca metadatos en Steam Workshop para un artículo específico.
    /// </summary>
    /// <param name="workshopId">ID de Steam.</param>
    /// <returns>Falso si no se pudo obtener información.</returns>
    Task<bool> FetchSteamMetadataAsync(string workshopId);
}

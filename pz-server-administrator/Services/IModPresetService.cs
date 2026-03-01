using System.Collections.Generic;
using System.Threading.Tasks;
using pz_server_administrator.Data.Database.Mods;

namespace pz_server_administrator.Services;

/// <summary>
/// Interfaz para la gestión de Presets (Perfiles) de mods.
/// </summary>
public interface IModPresetService
{
    /// <summary>
    /// Crea un preset a partir de la lista de mods activos y su orden.
    /// </summary>
    Task SavePresetAsync(string name, string? description, List<ModInstance> activeMods);

    /// <summary>
    /// Guarda un preset completo (útil para importación).
    /// </summary>
    Task SavePresetAsync(ModPreset preset);

    /// <summary>
    /// Lista todos los presets disponibles en el disco.
    /// </summary>
    Task<List<ModPreset>> GetAllPresetsAsync();

    /// <summary>
    /// Elimina un preset por su nombre.
    /// </summary>
    Task DeletePresetAsync(string name);

    /// <summary>
    /// Aplica un preset a la base de datos local (marcar activos y reordenar).
    /// </summary>
    Task ApplyPresetAsync(string name);
}

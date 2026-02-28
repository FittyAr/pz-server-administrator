namespace pz_server_administrator.Data.Database.Mods;

/// <summary>
/// Categorías de mods basadas en la jerarquía estándar de Project Zomboid.
/// </summary>
public enum ModCategory
{
    /// <summary>
    /// Librerías base necesarias para otros mods (ej: Moodles Framework).
    /// </summary>
    Framework = 0,

    /// <summary>
    /// Assets visuales y auditivos (Texturas, Sonidos).
    /// </summary>
    Resources = 1,

    /// <summary>
    /// Nuevos territorios y cambios en el mapa.
    /// </summary>
    Maps = 2,

    /// <summary>
    /// Nuevos vehículos añadidos por mods.
    /// </summary>
    Vehicles = 3,

    /// <summary>
    /// Tweaks de lógica pura que no añaden assets visuales.
    /// </summary>
    CodeOnly = 4,

    /// <summary>
    /// Equipamiento adicional y cambios en la interfaz de usuario.
    /// </summary>
    ClothingInterface = 5,

    /// <summary>
    /// Traducciones.
    /// </summary>
    Localization = 6,

    /// <summary>
    /// Mods generales no categorizados.
    /// </summary>
    Other = 7
}

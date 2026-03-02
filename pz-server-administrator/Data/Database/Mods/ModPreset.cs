using System;
using System.Collections.Generic;

namespace pz_server_administrator.Data.Database.Mods;

/// <summary>
/// Estructura de un perfil de mods para exportar/importar (Presets).
/// </summary>
public class ModPreset
{
    /// <summary>
    /// ID único del preset (UUID).
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Nombre descriptivo del preset (ej: "Hardcore Roleplay").
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Autor del preset (si es compartido).
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Indica si el preset es compartido con la comunidad.
    /// </summary>
    public bool IsShared { get; set; } = false;

    /// <summary>
    /// Descripción breve de lo que incluye.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Lista de mods incluidos en este preset y sus órdenes.
    /// </summary>
    public List<ModPresetEntry> Mods { get; set; } = new();

    /// <summary>
    /// Fecha de creación del preset.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Entrada individual de un preset.
/// </summary>
public class ModPresetEntry
{
    public string WorkshopId { get; set; } = null!;
    public string ModId { get; set; } = null!;
    public int SortOrder { get; set; }
}

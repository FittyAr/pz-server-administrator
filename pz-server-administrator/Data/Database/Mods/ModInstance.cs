using System;
using System.Collections.Generic;

namespace pz_server_administrator.Data.Database.Mods;

/// <summary>
/// Representa una parte activable individualmente dentro del Workshop Item (descubierto por mod.info).
/// </summary>
public partial class ModInstance
{
    /// <summary>
    /// ID interno del mod (ej: "frusedcars", "Hydrocraft").
    /// </summary>
    public string ModId { get; set; } = null!;

    /// <summary>
    /// Nombre visible del mod (ej: "Filibuster Rhymes' Used Cars! - Lorena Edition").
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Categoría basada en la jerarquía técnica de carga.
    /// </summary>
    public ModCategory Category { get; set; } = ModCategory.Other;

    /// <summary>
    /// Indica si está actualmente activado en el servidor Zomboid.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// ID del Workshop Item de Steam al cual pertenece (FK).
    /// </summary>
    public string WorkshopItemId { get; set; } = null!;

    /// <summary>
    /// Referencia al Workshop Item contenedor.
    /// </summary>
    public virtual WorkshopItem WorkshopItem { get; set; } = null!;
}

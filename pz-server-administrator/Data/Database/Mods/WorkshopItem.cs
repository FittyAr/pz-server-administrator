using System;
using System.Collections.Generic;

namespace pz_server_administrator.Data.Database.Mods;

/// <summary>
/// Representa el elemento de Steam Workshop (el "contenedor" de mods).
/// </summary>
public partial class WorkshopItem
{
    /// <summary>
    /// ID del Workshop de Steam (primario).
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Título legible del Workshop Item.
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Descripción extendida (extraída de Steam).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// URL o ruta local a la miniatura/poster.
    /// </summary>
    public string? ThumbnailPath { get; set; }

    /// <summary>
    /// Fecha de la última actualización en Steam.
    /// </summary>
    public DateTime? LastUpdated { get; set; }

    /// <summary>
    /// Fecha de última modificación local detectada.
    /// </summary>
    public DateTime? LocalUpdatedAt { get; set; }

    /// <summary>
    /// Hash de contenido para detectar actualizaciones del servidor.
    /// </summary>
    public string? VersionHash { get; set; }

    /// <summary>
    /// Indica si hay una versión más reciente en Steam que la local.
    /// </summary>
    public bool IsUpdateAvailable { get; set; } = false;

    /// <summary>
    /// Orden de descarga o carga del WorkshopItem.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Componentes activables individualmente dentro de este Workshop Item.
    /// </summary>
    public virtual ICollection<ModInstance> Instances { get; set; } = new List<ModInstance>();
}

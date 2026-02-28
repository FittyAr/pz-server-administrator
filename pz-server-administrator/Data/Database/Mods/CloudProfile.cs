using System;

namespace pz_server_administrator.Data.Database.Mods;

/// <summary>
/// Configuración vinculada al usuario para telemetría y sincronización en la nube.
/// </summary>
public class CloudProfile
{
    /// <summary>
    /// ID primaria (por defecto 1).
    /// </summary>
    public int Id { get; set; } = 1;

    /// <summary>
    /// Clave de API para el servicio centralizado.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Indica si el reporte automático de configuraciones funcionales está activado.
    /// </summary>
    public bool AutoReport { get; set; }

    /// <summary>
    /// Indica si se debe sincronizar automáticamente la configuración de mods.
    /// </summary>
    public bool CloudSyncEnabled { get; set; }

    /// <summary>
    /// Fecha de la última sincronización en la nube.
    /// </summary>
    public DateTime? LastSync { get; set; }

    /// <summary>
    /// Indica si el Agente de IA tiene permiso para aplicar cambios automáticamente.
    /// </summary>
    public bool AiAutoFixEnabled { get; set; }
}

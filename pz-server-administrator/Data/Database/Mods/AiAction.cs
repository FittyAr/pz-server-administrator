using System.Collections.Generic;

namespace pz_server_administrator.Data.Database.Mods;

/// <summary>
/// Representa una acción sugerida o ejecutada por el Agente de IA.
/// </summary>
public class AiAction
{
    public AiActionType Type { get; set; }

    /// <summary>
    /// ID del Workshop o ModID afectado.
    /// </summary>
    public string TargetId { get; set; } = null!;

    /// <summary>
    /// Parámetros adicionales (ej: "new_order": "5").
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; } = new();

    /// <summary>
    /// Explicación técnica de la IA sobre por qué propone esta acción.
    /// </summary>
    public string Reason { get; set; } = null!;

    /// <summary>
    /// Nivel de confianza de la IA en esta acción (0-1).
    /// </summary>
    public double Confidence { get; set; } = 1.0;
}

public enum AiActionType
{
    Deactivate,
    Activate,
    Reorder,
    UpdateMetadata,
    FixConfig,
    Recommendation,
    RequestDeepScan,
    RequestFile
}

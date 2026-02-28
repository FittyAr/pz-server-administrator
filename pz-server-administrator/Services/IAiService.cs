using System.Threading.Tasks;
using System.Collections.Generic;
using pz_server_administrator.Data.Database.Mods;

namespace pz_server_administrator.Services;

/// <summary>
/// Interfaz para el servicio de inteligencia artificial.
/// Proporciona diagnósticos de mods, sugerencias de orden y explicaciones de errores.
/// </summary>
public interface IAiService
{
    /// <summary>
    /// Analiza una lista de mods en busca de conflictos potenciales basados en sus metadatos.
    /// </summary>
    /// <param name="activeMods">Lista de mods actualmente activos.</param>
    /// <returns>Una cadena con el diagnóstico y advertencias.</returns>
    Task<string> AnalyzeModConflictsAsync(IEnumerable<ModInstance> activeMods);

    /// <summary>
    /// Sugiere un orden de carga óptimo para la lista de mods proporcionada.
    /// </summary>
    /// <param name="activeMods">Lista de mods a ordenar.</param>
    /// <returns>La lista de mods reordenada siguiendo criterios técnicos de estabilidad.</returns>
    Task<List<ModInstance>> SuggestOptimizedOrderAsync(IEnumerable<ModInstance> activeMods);

    /// <summary>
    /// Resuelve dudas del usuario sobre un error específico del servidor.
    /// </summary>
    /// <param name="errorLog">El fragmento del log de error.</param>
    /// <returns>Una explicación legible y posibles soluciones.</returns>
    Task<string> ExplainErrorAsync(string errorLog);

    /// <summary>
    /// Analiza un log de error (server-console.txt) para encontrar la causa de un crash o error de mods.
    /// </summary>
    Task<string> AnalyzeLogAsync(string logContent);

    /// <summary>
    /// Realiza un análisis profundo de la configuración actual y propone acciones correctivas.
    /// </summary>
    Task<List<AiAction>> AnalyzeAndFixAsync(IEnumerable<ModInstance> currentMods, string? logContext = null);
}

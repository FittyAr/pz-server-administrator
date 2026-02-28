using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using pz_server_administrator.Data.Database.Mods;

namespace pz_server_administrator.Services;

/// <summary>
/// Implementación base (Mock/Híbrida) del servicio de Inteligencia Artificial para el Sprint 3.
/// Se expandirá con integración real de LLMs (Gemini/OpenAI) mediante API Key.
/// </summary>
public class AiService : IAiService
{
    private readonly ILogger<AiService> _logger;
    private readonly IConfigurationService _configService;

    public AiService(ILogger<AiService> logger, IConfigurationService configService)
    {
        _logger = logger;
        _configService = configService;
    }

    /// <summary>
    /// Analiza conflictos basándose en heurísticas y (futuro) LLM.
    /// </summary>
    public async Task<string> AnalyzeModConflictsAsync(IEnumerable<ModInstance> activeMods)
    {
        _logger.LogInformation("[AI] Iniciando análisis de conflictos...");
        await Task.Delay(1000); // Simulando procesamiento

        var conflicts = new List<string>();
        var mods = activeMods.ToList();

        // Heurística simple: Mapas solpados
        var maps = mods.Where(m => m.Category == ModCategory.Maps).ToList();
        if (maps.Count > 1)
        {
            conflicts.Add($"- Alerta: Tienes {maps.Count} mapas activos. El orden de carga decidirá qué mapa sobreescribe las celdas coincidentes.");
        }

        // Heurística simple: Frameworks faltantes (futuro)

        if (!conflicts.Any())
            return "No se detectaron conflictos críticos evidentes. ¡Tu lista parece estable!";

        return "Diagnóstico de IA:\n" + string.Join("\n", conflicts);
    }

    public async Task<List<ModInstance>> SuggestOptimizedOrderAsync(IEnumerable<ModInstance> activeMods)
    {
        _logger.LogInformation("[AI] Calculando orden óptimo...");
        await Task.Delay(500);

        // Seguir el estándar técnico de Zomboid (definido en PROJECT_PLAN.md)
        return activeMods
            .OrderBy(m => (int)m.Category)
            .ThenBy(m => m.Name)
            .ToList();
    }

    public async Task<string> ExplainErrorAsync(string errorLog)
    {
        await Task.Yield();
        if (string.IsNullOrEmpty(errorLog)) return "Proporciona el error del log para analizarlo.";

        // Aquí iría la llamada al LLM
        return "Análisis preliminar: El error parece estar relacionado con un archivo Lua inexistente o mal cargado. Revisa tus mods de lógica.";
    }
}

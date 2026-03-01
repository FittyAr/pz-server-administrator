using pz_server_administrator.Data.Database.Mods;
using pz_server_administrator.Models;

namespace pz_server_administrator.Services.Ai;

public class OllamaProvider : IAiProvider
{
    public Task<string> AnalyzeModConflictsAsync(IEnumerable<ModInstance> activeMods, AiConfiguration config)
        => Task.FromResult("Ollama: La detección de conflictos aún no está disponible.");

    public Task<string> AnalyzeLogAsync(string logContent, AiConfiguration config)
        => Task.FromResult("Ollama: El análisis de logs aún no está disponible.");

    public Task<List<AiAction>> AnalyzeAndFixAsync(IEnumerable<ModInstance> currentMods, AiConfiguration config, string? logContext = null)
        => Task.FromResult(new List<AiAction>());

    public Task<bool> ValidateApiKeyAsync(string apiKey)
        => Task.FromResult(true); // Ollama no suele requerir API Key por defecto

    public Task<string> GetCompletionAsync(string systemPrompt, string userPrompt, AiConfiguration config)
        => Task.FromResult("Ollama: El completado de chat aún no está disponible.");
}

using pz_server_administrator.Data.Database.Mods;
using pz_server_administrator.Models;

namespace pz_server_administrator.Services.Ai;

public interface IAiProvider
{
    Task<string> AnalyzeModConflictsAsync(IEnumerable<ModInstance> activeMods, AiConfiguration config);
    Task<string> AnalyzeLogAsync(string logContent, AiConfiguration config);
    Task<List<AiAction>> AnalyzeAndFixAsync(IEnumerable<ModInstance> currentMods, AiConfiguration config, string? logContext = null);
    Task<bool> ValidateApiKeyAsync(string apiKey);
    Task<string> GetCompletionAsync(string systemPrompt, string userPrompt, AiConfiguration config);
}

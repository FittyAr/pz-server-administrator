using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using pz_server_administrator.Data.Database.Mods;
using pz_server_administrator.Models;

namespace pz_server_administrator.Services.Ai;

public class GeminiProvider : IAiProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GeminiProvider> _logger;

    public GeminiProvider(IHttpClientFactory httpClientFactory, ILogger<GeminiProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<string> AnalyzeModConflictsAsync(IEnumerable<ModInstance> activeMods, AiConfiguration config)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var model = string.IsNullOrEmpty(config.ModelName) ? "gemini-1.5-flash" : config.ModelName;
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={config.ApiKey}";

            var modData = string.Join("\n", activeMods.Select(m => $"- ID: {m.ModId}, Name: {m.Name}, Category: {m.Category}"));

            var prompt = new
            {
                contents = new[]
                {
                    new {
                        parts = new[]
                        {
                            new { text = $"Eres un experto en el juego Project Zomboid y su arquitectura de mods. Analiza el siguiente orden de carga y detecta conflictos potenciales de carga (ej: frameworks o localizaciones mal posicionados, mapas encimados). Responde de forma técnica y breve en español.\n\nMODS ACTIVOS:\n{modData}" }
                        }
                    }
                }
            };

            var response = await client.PostAsJsonAsync(url, prompt);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<dynamic>();
                string? text = result?.candidates[0].content.parts[0].text;
                return "🤖 Informe de Inteligencia Artificial (Gemini Pro):\n\n" + (text ?? "No se recibió respuesta legible de la IA.");
            }

            return "❌ Error al conectar con Gemini API. Revisa tu API Key o conexión.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GeminiProvider] Error calling Gemini.");
            return "❌ Excepción contactando al servicio de IA externo.";
        }
    }

    public async Task<string> AnalyzeLogAsync(string logContent, AiConfiguration config)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var model = string.IsNullOrEmpty(config.ModelName) ? "gemini-1.5-flash" : config.ModelName;
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={config.ApiKey}";

            var prompt = new
            {
                contents = new[]
                {
                    new {
                        parts = new[]
                        {
                            new { text = $"Analiza el siguiente fragmento de log de un servidor de Project Zomboid. Identifica errores fatales, excepciones de Lua o problemas de mods. Explica la causa y qué mod podría estar fallando. Responde en español.\n\nLOG:\n{logContent}" }
                        }
                    }
                }
            };

            var response = await client.PostAsJsonAsync(url, prompt);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<dynamic>();
                string? text = result?.candidates[0].content.parts[0].text;
                return "🤖 Diagnóstico de Log (Gemini Pro):\n\n" + (text ?? "No se pudo interpretar el log.");
            }
            return "❌ Error conectando con el servicio de IA.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GeminiProvider] Error analyzing log.");
            return "❌ Excepción durante el análisis del log.";
        }
    }

    public Task<List<AiAction>> AnalyzeAndFixAsync(IEnumerable<ModInstance> currentMods, AiConfiguration config, string? logContext = null)
    {
        // El ciclo agéntico se maneja en el servicio base usando GetCompletionAsync.
        throw new NotImplementedException("El ciclo agéntico se maneja en el servicio base.");
    }

    public async Task<string> GetCompletionAsync(string systemPrompt, string userPrompt, AiConfiguration config)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var model = string.IsNullOrEmpty(config.ModelName) ? "gemini-1.5-flash" : config.ModelName;
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={config.ApiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new {
                        parts = new[]
                        {
                            new { text = systemPrompt },
                            new { text = userPrompt }
                        }
                    }
                },
                generationConfig = new
                {
                    response_mime_type = config.AiAutoFixEnabled ? "application/json" : "text/plain"
                }
            };

            var response = await client.PostAsJsonAsync(url, requestBody);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<dynamic>();
                string? text = result?.candidates[0].content.parts[0].text;
                return text ?? string.Empty;
            }
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GeminiProvider] Error in GetCompletionAsync.");
            return string.Empty;
        }
    }

    public async Task<bool> ValidateApiKeyAsync(string apiKey)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

            var testPrompt = new { contents = new[] { new { parts = new[] { new { text = "ping" } } } } };
            var response = await client.PostAsJsonAsync(url, testPrompt);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

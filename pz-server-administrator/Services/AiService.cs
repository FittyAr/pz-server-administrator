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
    private readonly IModDiscoveryService _modDiscovery;
    private readonly IHttpClientFactory _httpClientFactory;

    public AiService(
        ILogger<AiService> logger,
        IConfigurationService configService,
        IModDiscoveryService modDiscovery,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configService = configService;
        _modDiscovery = modDiscovery;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Analiza conflictos basándose en heurísticas y (futuro) LLM.
    /// </summary>
    public async Task<string> AnalyzeModConflictsAsync(IEnumerable<ModInstance> activeMods)
    {
        _logger.LogInformation("[AI] Iniciando análisis exhaustivo de conflictos...");

        var profile = await _modDiscovery.GetCloudProfileAsync();
        if (!string.IsNullOrEmpty(profile?.ApiKey) && profile.ApiKey.StartsWith("AI-")) // Prefijo para identificar API Keys de IA
        {
            return await AnalyzeWithGeminiAsync(activeMods, profile.ApiKey.Replace("AI-", ""));
        }

        await Task.Delay(1500); // Simulando procesamiento profundo

        var reports = new List<string>();
        var mods = activeMods.OrderBy(m => m.Order).ToList();

        if (!mods.Any()) return "No tienes mods activos que analizar.";

        // 1. Validar jerarquía de carga
        for (int i = 0; i < mods.Count; i++)
        {
            for (int j = i + 1; j < mods.Count; j++)
            {
                // Un mod cargando después que otro de una categoría superior (ej: Framework después de Map)
                if ((int)mods[i].Category > (int)mods[j].Category)
                {
                    reports.Add($"- ⚠️ Orden potencial: '{mods[j].ModId}' ({mods[j].Category}) está cargando después de '{mods[i].ModId}' ({mods[i].Category}). Considera subir el tipo '{mods[j].Category}' arriba.");
                }
            }
        }

        // 2. Localización al final
        var lastMod = mods.Last();
        if (mods.Any(m => m.Category == ModCategory.Localization) && lastMod.Category != ModCategory.Localization)
        {
            reports.Add("- 💡 Recomendación: Los mods de traducción (Localization) suelen ir al final del orden para sobreescribir textos correctamente.");
        }

        // 3. Frameworks al inicio
        var firstMod = mods.First();
        if (mods.Any(m => m.Category == ModCategory.Framework) && firstMod.Category != ModCategory.Framework)
        {
            reports.Add("- 💡 Recomendación: Los Frameworks e IDs de librerías deben cargar lo más arriba posible.");
        }

        // 4. Mapas duplicados
        var maps = mods.Where(m => m.Category == ModCategory.Maps).ToList();
        if (maps.Count > 1)
        {
            reports.Add($"- 🗺️ Nota de Mapas: Tienes {maps.Count} mapas activos. Recuerda que el que esté más arriba será sobreescrito por los de abajo si comparten celdas.");
        }

        if (!reports.Any())
            return "✅ El análisis no encontró problemas estructurales. Tu orden de carga sigue los estándares recomendados.";

        return "🔍 Informe de Diagnóstico Estructural:\n\n" + string.Join("\n", reports) + "\n\n*Nota: Este análisis es heurístico. Los desarrolladores de mods podrían tener requisitos específicos.*";
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

    private async Task<string> AnalyzeWithGeminiAsync(IEnumerable<ModInstance> mods, string apiKey)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

            var modData = string.Join("\n", mods.Select(m => $"- ID: {m.ModId}, Name: {m.Name}, Category: {m.Category}"));

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
                // Navegar por el JSON de Gemini para extraer el texto (v1beta)
                string? text = result?.candidates[0].content.parts[0].text;
                return "🤖 Informe de Inteligencia Artificial (Gemini Pro):\n\n" + (text ?? "No se recibió respuesta legible de la IA.");
            }

            return "❌ Error al conectar con Gemini API. Revisa tu API Key o conexión.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AI] Error llamando a Gemini.");
            return "❌ Excepción contactando al servicio de IA externo.";
        }
    }
}

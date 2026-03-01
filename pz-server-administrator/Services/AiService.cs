using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
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

    public async Task<string> AnalyzeLogAsync(string logContent)
    {
        if (string.IsNullOrEmpty(logContent)) return "El log está vacío.";

        var profile = await _modDiscovery.GetCloudProfileAsync();
        if (string.IsNullOrEmpty(profile?.ApiKey) || !profile.ApiKey.StartsWith("AI-"))
        {
            return "⚠️ Se requiere una Gemini API Key (con prefijo AI-) en los ajustes para realizar análisis de logs profundos.";
        }

        try
        {
            var apiKey = profile.ApiKey.Replace("AI-", "");
            var client = _httpClientFactory.CreateClient();
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

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
            _logger.LogError(ex, "[AI] Error analizando log.");
            return "❌ Excepción durante el análisis del log.";
        }
    }

    public async Task<List<AiAction>> AnalyzeAndFixAsync(IEnumerable<ModInstance> currentMods, string? logContext = null, IProgress<string>? progress = null)
    {
        var profile = await _modDiscovery.GetCloudProfileAsync();
        if (string.IsNullOrEmpty(profile?.ApiKey) || !profile.ApiKey.StartsWith("AI-"))
        {
            progress?.Report("Falta API Key de Gemini. Canceling.");
            return new List<AiAction> {
                new AiAction {
                    Type = AiActionType.Recommendation,
                    Reason = "Configura una Gemini API Key (AI-) para habilitar el Agente de Diagnóstico Autónomo."
                }
            };
        }

        try
        {
            var apiKey = profile.ApiKey.Replace("AI-", "");
            var client = _httpClientFactory.CreateClient();
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

            var modData = string.Join("\n", currentMods.Select(m => $"- ID: {m.ModId}, Category: {m.Category}, Workshop: {m.WorkshopItemId}, Order: {m.Order}"));
            var dynamicContext = logContext != null ? $"CONTEXTO INICIAL:\n{logContext}\n" : "";

            int maxLoops = 3;
            int currentLoop = 0;

            while (currentLoop < maxLoops)
            {
                currentLoop++;
                progress?.Report($"Evaluando configuración... (Ciclo {currentLoop}/{maxLoops})");

                var systemPrompt = @"Actúa como un Ingeniero de Sistemas Senior y experto en Inteligencia Artificial especializado en el motor de Project Zomboid (Java/Lua). 
Tu objetivo es analizar la lista de mods y el contexto para proponer una lista de acciones técnicas que estabilicen el servidor.

REGLAS TÉCNICAS:
1. Frameworks/Core Libs (Tsar's Common Library, Mod Options, etc.) DEBEN cargar primero (Categoría Framework).
2. Mapas (Categoría Maps) cargan después de los Frameworks.
3. Localizaciones (Categoría Localization) siempre al final.

ADQUISICIÓN DE DATOS (AGENTIC LOOP):
Si necesitas más información antes de decidir, puedes devolver UNA de estas acciones especiales:
- RequestDeepScan: Solicita un escaneo completo de archivos LUA superpuestos entre los mods activos. Usa ""Parameters"": {}.
- RequestFile: Solicita leer el contenido de un archivo específico de un mod. Usa ""Parameters"": { ""path"": ""ruta relativa o absoluta"" }.

Responde ÚNICAMENTE con un JSON válido (Array de objetos):
[
  {
    ""Type"": ""Deactivate"" | ""Activate"" | ""Reorder"" | ""FixConfig"" | ""RequestDeepScan"" | ""RequestFile"",
    ""TargetId"": ""ID del mod afectado o 'system' si es global"",
    ""Parameters"": { ""new_order"": ""valor_numerico"", ""path"": ""ruta"" },
    ""Reason"": ""Explicación técnica en español"",
    ""Confidence"": 0.95
  }
]
Si no hay problemas y no necesitas datos, devuelve un array vacío [].";

                var fullPrompt = $"MODS ACTUALES:\n{modData}\n\nCONTEXTO ACUMULADO:\n{dynamicContext}\n\nAnaliza y genera las acciones necesarias en formato JSON (Intento {currentLoop}/{maxLoops}).";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new {
                            parts = new[]
                            {
                                new { text = systemPrompt },
                                new { text = fullPrompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        response_mime_type = "application/json"
                    }
                };

                var response = await client.PostAsJsonAsync(url, requestBody);
                if (!response.IsSuccessStatusCode) break; // Error de API

                var result = await response.Content.ReadFromJsonAsync<dynamic>();
                string? jsonText = result?.candidates[0].content.parts[0].text;

                if (string.IsNullOrEmpty(jsonText)) break;

                // Extraemos el JSON puro por si Gemini incluye tildes invertidas
                try
                {
                    var actions = JsonSerializer.Deserialize<List<AiAction>>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

                    if (actions.Count == 0) return actions; // IA dice que está todo bien

                    // Revisar si son acciones terminales o peticiones de información
                    var requests = actions.Where(a => a.Type == AiActionType.RequestDeepScan || a.Type == AiActionType.RequestFile).ToList();

                    if (requests.Count == 0 || currentLoop >= maxLoops)
                    {
                        // Son acciones finales a aplicar, o ya no podemos hacer más iteraciones
                        progress?.Report("Análisis finalizado, generando reporte.");
                        return actions.Where(a => a.Type != AiActionType.RequestDeepScan && a.Type != AiActionType.RequestFile).ToList();
                    }

                    // Procesar solicitudes de IA
                    foreach (var req in requests)
                    {
                        if (req.Type == AiActionType.RequestDeepScan)
                        {
                            _logger.LogInformation("[AI Agent] IA solicitó Deep Scan de archivos...");
                            progress?.Report("Agente Solicitando: Escaneo Profundo de Archivos (Deep Scan LUA)...");
                            var conflicts = await _modDiscovery.ScanDeepFileConflictsAsync();
                            dynamicContext += "\nRESULTADOS DE DEEP SCAN (Archivos sobrescritos):\n";
                            if (conflicts.Count == 0) dynamicContext += "No se encontraron conflictos de sobreescritura LUA/INI.\n";
                            else
                            {
                                foreach (var kvp in conflicts)
                                    dynamicContext += $"- {kvp.Key} es proporcionado por: {string.Join(", ", kvp.Value)}\n";
                            }
                        }
                        else if (req.Type == AiActionType.RequestFile && req.Parameters.TryGetValue("path", out var path))
                        {
                            _logger.LogInformation("[AI Agent] IA solicitó leer archivo: {Path}", path);
                            progress?.Report($"Agente Solicitando: Leer contenido de '{path}'...");
                            var content = await _modDiscovery.GetFileContentAsync(path);
                            // Tomamos un fragmento para no pasar los límites de tokens
                            if (content.Length > 2000) content = content.Substring(0, 2000) + "... (truncado)";
                            dynamicContext += $"\nCONTENIDO DE {path}:\n{content}\n";
                        }
                    }
                    progress?.Report("Enviando resultados de la investigación al agente...");
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "[AI Agent] Error parseando respuesta JSON de la IA: {JSON}", jsonText);
                    break;
                }
            }

            return new List<AiAction>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AI Agent] Error en ciclo agéntico.");
            return new List<AiAction> { new AiAction { Type = AiActionType.Recommendation, Reason = $"Error en el Agente: {ex.Message}" } };
        }
    }
}

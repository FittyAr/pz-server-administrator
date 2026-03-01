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
    private readonly pz_server_administrator.Services.Ai.AiProviderFactory _providerFactory;

    public AiService(
        ILogger<AiService> logger,
        IConfigurationService configService,
        IModDiscoveryService modDiscovery,
        pz_server_administrator.Services.Ai.AiProviderFactory providerFactory)
    {
        _logger = logger;
        _configService = configService;
        _modDiscovery = modDiscovery;
        _providerFactory = providerFactory;
    }

    /// <summary>
    /// Analiza conflictos basándose en heurísticas y (futuro) LLM.
    /// </summary>
    public async Task<string> AnalyzeModConflictsAsync(IEnumerable<ModInstance> activeMods)
    {
        _logger.LogInformation("[AI] Iniciando análisis exhaustivo de conflictos...");

        var config = _configService.GetConfiguration().Ai;
        if (config.IsApiKeyValid && !string.IsNullOrEmpty(config.ApiKey))
        {
            var provider = _providerFactory.GetProvider(config.Provider);
            return await provider.AnalyzeModConflictsAsync(activeMods, config);
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

        // Seguir el estándar técnico de Zomboid
        return activeMods
            .OrderBy(m => (int)m.Category)
            .ThenBy(m => m.Name)
            .ToList();
    }

    public async Task<string> ExplainErrorAsync(string errorLog)
    {
        if (string.IsNullOrEmpty(errorLog)) return "Proporciona el error del log para analizarlo.";

        var config = _configService.GetConfiguration().Ai;
        if (config.IsApiKeyValid && !string.IsNullOrEmpty(config.ApiKey))
        {
            var provider = _providerFactory.GetProvider(config.Provider);
            return await provider.AnalyzeLogAsync(errorLog, config);
        }

        return "Análisis preliminar (Heurístico): El error parece estar relacionado con un archivo Lua o de configuración. Valida una API Key para un diagnóstico detallado.";
    }

    public async Task<string> AnalyzeLogAsync(string logContent)
    {
        if (string.IsNullOrEmpty(logContent)) return "El log está vacío.";

        var config = _configService.GetConfiguration().Ai;
        if (!config.IsApiKeyValid || string.IsNullOrEmpty(config.ApiKey))
        {
            return "⚠️ Se requiere una API Key de IA validada en los ajustes para realizar análisis de logs profundos.";
        }

        try
        {
            var provider = _providerFactory.GetProvider(config.Provider);
            return await provider.AnalyzeLogAsync(logContent, config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AI] Error analizando log.");
            return "❌ Excepción durante el análisis del log.";
        }
    }

    public async Task<List<AiAction>> AnalyzeAndFixAsync(IEnumerable<ModInstance> currentMods, string? logContext = null, IProgress<string>? progress = null)
    {
        var config = _configService.GetConfiguration().Ai;
        if (!config.IsApiKeyValid || string.IsNullOrEmpty(config.ApiKey))
        {
            progress?.Report("Falta API Key de IA validada. Canceling.");
            return new List<AiAction> {
                new AiAction {
                    Type = AiActionType.Recommendation,
                    Reason = "Ingresa y valida una API Key en los ajustes para habilitar el Agente de Diagnóstico Autónomo."
                }
            };
        }

        try
        {
            var provider = _providerFactory.GetProvider(config.Provider);
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

                string jsonText = await provider.GetCompletionAsync(systemPrompt, fullPrompt, config);

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

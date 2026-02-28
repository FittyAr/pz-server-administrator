using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using pz_server_administrator.Data.Database.Mods;

namespace pz_server_administrator.Services;

/// <summary>
/// Conexión con el servicio central de la comunidad (v4: Telemetría y Ecosistema).
/// </summary>
public class CommunityService : ICommunityService
{
    private readonly ILogger<CommunityService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public CommunityService(ILogger<CommunityService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task ReportConfigurationAsync(string apiKey, List<ModInstance> activeMods)
    {
        if (string.IsNullOrEmpty(apiKey)) return;

        try
        {
            _logger.LogInformation("[Community] Reportando configuración de {Count} mods activos...", activeMods.Count);

            // Simulación de envío anónimo o identificado si hay API Key real.
            // En el futuro: await client.PostAsJsonAsync("https://api.pz-admin.com/v1/report", data);

            await Task.Delay(500); // Simulación de red
            _logger.LogInformation("[Community] Configuración reportada con éxito.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Community] Falló el reporte de telemetría.");
        }
    }

    public async Task<string> GetGlobalRecommendationsAsync()
    {
        // Mock de recomendaciones globales basada en comunidad.
        return await Task.FromResult("🔥 Mod Tendencia: 'Common Sense' - 98% de servidores lo usan.");
    }

    public async Task SyncIncompatibilitiesAsync(string apiKey)
    {
        // En el futuro, recuperaría de la nube pares de mods que suelen dar error.
        _logger.LogInformation("[Community] Sincronizando presets de incompatibilidad globales...");
        await Task.Delay(300);
    }
}

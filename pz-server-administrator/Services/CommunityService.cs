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

    public async Task<bool> UploadPresetAsync(string apiKey, ModPreset preset)
    {
        if (string.IsNullOrEmpty(apiKey)) return false;

        _logger.LogInformation("[Community] Subiendo preset '{Name}' a la nube...", preset.Name);
        // await client.PostAsJsonAsync("https://api.pz-admin.com/v1/presets", preset);
        await Task.Delay(1000);
        return true;
    }

    public async Task<List<ModPreset>> GetCommunityPresetsAsync()
    {
        _logger.LogInformation("[Community] Recuperando presets públicos...");
        await Task.Delay(500);

        return new List<ModPreset>
        {
            new ModPreset
            {
                Id = "shared-1",
                Name = "Vanilla Survival Pack",
                Description = "Recomendado por la comunidad: Mejora la experiencia base sin romperla.",
                Author = "Admin-Global",
                IsShared = true,
                Mods = new List<ModPresetEntry>()
            },
            new ModPreset
            {
                Id = "shared-2",
                Name = "Hardcore Realistic",
                Description = "Solo para expertos. Incluye mods de sed, cansancio dinámico y zombies ultra-sensibles.",
                Author = "Admin-Global",
                IsShared = true,
                Mods = new List<ModPresetEntry>()
            }
        };
    }

    public async Task<bool> ValidateApiKeyAsync(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey)) return false;

        try
        {
            // Simulación de validación (v4: Realizaría un GET a /auth/verify)
            await Task.Delay(800);
            // Consideramos válida cualquier clave que empiece por zsm- o simplemente que tenga longitud
            return apiKey.Length > 10;
        }
        catch
        {
            return false;
        }
    }
}

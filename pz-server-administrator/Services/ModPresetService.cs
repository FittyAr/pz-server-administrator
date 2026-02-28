using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using pz_server_administrator.Data.Database.Mods;

namespace pz_server_administrator.Services;

/// <summary>
/// Implementación de la gestión de Presets (Perfiles) de mods (Sprint 5: Automatización).
/// </summary>
public class ModPresetService : IModPresetService
{
    private readonly ILogger<ModPresetService> _logger;
    private readonly string _presetsDir;
    private readonly IDatabaseContextFactory _contextFactory;

    public ModPresetService(ILogger<ModPresetService> logger, IWebHostEnvironment env, IDatabaseContextFactory contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;

        // Misma lógica que ConfigurationService para volumen persistente
        var baseDir = Path.Combine(env.ContentRootPath, "config");
        _presetsDir = Path.Combine(baseDir, "presets");

        if (!Directory.Exists(_presetsDir))
        {
            Directory.CreateDirectory(_presetsDir);
        }
    }

    public async Task SavePresetAsync(string name, string? description, List<ModInstance> activeMods)
    {
        var preset = new ModPreset
        {
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            Mods = activeMods.Select(m => new ModPresetEntry
            {
                WorkshopId = m.WorkshopItemId,
                ModId = m.ModId,
                Order = m.Order
            }).ToList()
        };

        var fileName = $"{SanitizeFileName(name)}.json";
        var filePath = Path.Combine(_presetsDir, fileName);

        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(preset, options);

        await File.WriteAllTextAsync(filePath, json);
        _logger.LogInformation("[ModPreset] Preset guardado: {Name} ({Count} mods)", name, preset.Mods.Count);
    }

    public async Task<List<ModPreset>> GetAllPresetsAsync()
    {
        var presets = new List<ModPreset>();
        var files = Directory.GetFiles(_presetsDir, "*.json");

        foreach (var file in files)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var preset = JsonSerializer.Deserialize<ModPreset>(json);
                if (preset != null) presets.Add(preset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ModPreset] Error cargando preset desde {File}", file);
            }
        }

        return presets.OrderByDescending(p => p.CreatedAt).ToList();
    }

    public async Task DeletePresetAsync(string name)
    {
        var fileName = $"{SanitizeFileName(name)}.json";
        var filePath = Path.Combine(_presetsDir, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("[ModPreset] Preset eliminado: {Name}", name);
        }
        await Task.CompletedTask;
    }

    public async Task ApplyPresetAsync(string name)
    {
        var fileName = $"{SanitizeFileName(name)}.json";
        var filePath = Path.Combine(_presetsDir, fileName);

        if (!File.Exists(filePath)) throw new FileNotFoundException("El preset no existe.");

        var json = await File.ReadAllTextAsync(filePath);
        var preset = JsonSerializer.Deserialize<ModPreset>(json);
        if (preset == null) return;

        using var context = _contextFactory.CreateModsContext();
        if (context == null) return;

        // 1. Desactivar todos los mods actuales
        var allInstances = await context.ModInstances.ToListAsync();
        foreach (var inst in allInstances)
        {
            inst.IsActive = false;
        }

        // 2. Activar y ordenar según el preset
        foreach (var entry in preset.Mods)
        {
            var inst = allInstances.FirstOrDefault(i => i.ModId == entry.ModId && i.WorkshopItemId == entry.WorkshopId);
            if (inst != null)
            {
                inst.IsActive = true;
                inst.Order = entry.Order;
            }
            else
            {
                _logger.LogWarning("[ModPreset] Mod {ModId} (Workshop {WorkshopId}) no encontrado localmente. El preset lo requiere pero no está en la base de datos.", entry.ModId, entry.WorkshopId);
            }
        }

        await context.SaveChangesAsync();
        _logger.LogInformation("[ModPreset] Preset '{Name}' aplicado con éxito.", name);
    }

    private string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }
        return name.Replace(" ", "_");
    }
}

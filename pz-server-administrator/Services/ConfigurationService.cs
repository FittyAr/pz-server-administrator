using pz_server_administrator.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace pz_server_administrator.Services;

/// <summary>
/// Service for managing application configuration from appsettings.zsm.json
/// </summary>
public interface IConfigurationService
{
    ZsmConfiguration GetConfiguration();
    Task SaveConfigurationAsync(ZsmConfiguration configuration);
    void SaveConfiguration(ZsmConfiguration configuration);
    Task ReloadConfigurationAsync();
}

public class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private readonly string _configFilePath;
    private ZsmConfiguration _configuration;
    private readonly object _lock = new();

    public ConfigurationService(ILogger<ConfigurationService> logger, IWebHostEnvironment env)
    {
        _logger = logger;

        var configDir = FindConfigDirectory(env.ContentRootPath);
        _configFilePath = Path.Combine(configDir, "appsettings.zsm.json");
        _configuration = new ZsmConfiguration();

        _logger.LogInformation("[Configuration] Initializing with config file: {FilePath}", _configFilePath);

        LoadConfiguration();

        if (string.IsNullOrEmpty(_configuration.AppSettings.ServerDirectoryPath))
        {
            AutoDetectPzServer();
        }
    }

    private string FindConfigDirectory(string rootPath)
    {
        var searchPaths = new[]
        {
            Path.Combine(rootPath, "config"),
            Path.Combine(Directory.GetCurrentDirectory(), "config"),
        };

        foreach (var path in searchPaths)
        {
            if (Directory.Exists(path)) return path;
        }

        var dir = new DirectoryInfo(rootPath);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "config");
            if (Directory.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }

        // Fallback: create in current directory
        var fallback = Path.Combine(Directory.GetCurrentDirectory(), "config");
        Directory.CreateDirectory(fallback);
        return fallback;
    }

    private void AutoDetectPzServer()
    {
        _logger.LogInformation("[AutoDetect] Starting Project Zomboid server file discovery...");

        var potentialPaths = new List<string>
        {
            "/project-zomboid-config/Server",
            "/project-zomboid-config",
            "/data/Server",
            "/home/steam/Zomboid/Server",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Zomboid", "Server")
        };

        // If in container, scan root mounts
        if (IsRunningInContainer())
        {
            _logger.LogInformation("[AutoDetect] Container environment detected.");
        }

        foreach (var path in potentialPaths)
        {
            _logger.LogDebug("[AutoDetect] Checking path: {Path}", path);
            if (ScanDirectoryForServer(path)) return;

            // Try one level deeper if it's a common root
            if (Directory.Exists(path))
            {
                try
                {
                    foreach (var subDir in Directory.GetDirectories(path))
                    {
                        if (ScanDirectoryForServer(subDir)) return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug("[AutoDetect] Could not scan subdirectories of {Path}: {Message}", path, ex.Message);
                }
            }
        }

        _logger.LogWarning("[AutoDetect] Could not find any Project Zomboid server configuration (*.ini).");
    }

    private bool ScanDirectoryForServer(string path)
    {
        if (!Directory.Exists(path)) return false;

        try
        {
            var iniFiles = Directory.GetFiles(path, "*.ini");
            if (iniFiles.Length > 0)
            {
                var iniFile = iniFiles.FirstOrDefault(f => Path.GetFileName(f).Equals("servertest.ini", StringComparison.OrdinalIgnoreCase)) ?? iniFiles[0];
                var serverName = Path.GetFileNameWithoutExtension(iniFile);

                _logger.LogInformation("[AutoDetect] SUCCESS! Found PZ server at {Path}. Server name identified as: {ServerName}", path, serverName);

                lock (_lock)
                {
                    _configuration.AppSettings.ServerDirectoryPath = path;
                    _configuration.AppSettings.ActiveServer = serverName;
                }

                SaveConfiguration(_configuration);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("[AutoDetect] Error scanning {Path}: {Message}", path, ex.Message);
        }
        return false;
    }

    private bool IsRunningInContainer()
    {
        var isContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
            || File.Exists("/.dockerenv")
            || Environment.GetEnvironmentVariable("container") == "podman";

        return isContainer;
    }

    public ZsmConfiguration GetConfiguration()
    {
        lock (_lock) return _configuration;
    }

    public async Task SaveConfigurationAsync(ZsmConfiguration configuration)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(configuration, options);
            await File.WriteAllTextAsync(_configFilePath, json);
            lock (_lock) _configuration = configuration;
            _logger.LogInformation("[Configuration] Saved to {FilePath}", _configFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Configuration] Save failed to {FilePath}", _configFilePath);
            throw;
        }
    }

    public void SaveConfiguration(ZsmConfiguration configuration)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(configuration, options);
            File.WriteAllText(_configFilePath, json);
            lock (_lock) _configuration = configuration;
            _logger.LogInformation("[Configuration] Saved to {FilePath}", _configFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Configuration] Save failed to {FilePath}", _configFilePath);
            throw;
        }
    }

    public async Task ReloadConfigurationAsync() => await Task.Run(LoadConfiguration);

    private void LoadConfiguration()
    {
        try
        {
            if (!File.Exists(_configFilePath))
            {
                _logger.LogWarning("[Configuration] File not found: {FilePath}. Creating default.", _configFilePath);
                CreateDefaultConfiguration();
                SaveConfiguration(_configuration); // Persist default so it exits
                return;
            }

            var json = File.ReadAllText(_configFilePath);
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true };
            var configuration = JsonSerializer.Deserialize<ZsmConfiguration>(json, options);

            lock (_lock) _configuration = configuration ?? new ZsmConfiguration();
            _logger.LogInformation("[Configuration] Loaded from {FilePath}", _configFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Configuration] Load failed from {FilePath}. Using default.", _configFilePath);
            CreateDefaultConfiguration();
        }
    }

    private void CreateDefaultConfiguration()
    {
        lock (_lock)
        {
            _configuration = new ZsmConfiguration
            {
                AppSettings = new AppSettings { ServerDirectoryPath = "", ActiveServer = "", RCON = new RconSettings() },
                Users = new List<AuthUser>(),
                Roles = new Dictionary<string, RolePermissions>
                {
                    ["Guest"] = new RolePermissions { AllowConfigEdit = false, AllowRcon = false, AllowModManagement = false, AllowDatabaseWrite = false, AllowServerSwitch = false },
                    ["Moderator"] = new RolePermissions { AllowConfigEdit = false, AllowRcon = true, AllowModManagement = false, AllowDatabaseWrite = false, AllowServerSwitch = false },
                    ["Administrator"] = new RolePermissions { AllowConfigEdit = true, AllowRcon = true, AllowModManagement = true, AllowDatabaseWrite = true, AllowServerSwitch = true }
                }
            };
        }
    }
}
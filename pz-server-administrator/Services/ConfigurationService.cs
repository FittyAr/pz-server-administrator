using pz_server_administrator.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace pz_server_administrator.Services;

/// <summary>
/// Service for managing application configuration from appsettings.zsm.json
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Gets the current ZSM configuration
    /// </summary>
    ZsmConfiguration GetConfiguration();

    /// <summary>
    /// Saves the configuration to appsettings.zsm.json
    /// </summary>
    /// <param name="configuration">Configuration to save</param>
    Task SaveConfigurationAsync(ZsmConfiguration configuration);

    /// <summary>
    /// Saves the configuration to appsettings.zsm.json (synchronous)
    /// </summary>
    /// <param name="configuration">Configuration to save</param>
    void SaveConfiguration(ZsmConfiguration configuration);

    /// <summary>
    /// Reloads the configuration from file
    /// </summary>
    Task ReloadConfigurationAsync();
}

/// <summary>
/// Implementation of configuration service
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private readonly string _configFilePath;
    private ZsmConfiguration _configuration;
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of ConfigurationService
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="env">WebHost environment</param>
    public ConfigurationService(ILogger<ConfigurationService> logger, IWebHostEnvironment env)
    {
        _logger = logger;

        // Find config directory dynamically
        var configDir = FindConfigDirectory(env.ContentRootPath);
        _configFilePath = Path.Combine(configDir, "appsettings.zsm.json");
        _configuration = new ZsmConfiguration();

        _logger.LogInformation("Using configuration file at: {FilePath}", _configFilePath);

        // Load configuration on startup
        LoadConfiguration();

        // Auto-detect PZ server if not configured
        if (string.IsNullOrEmpty(_configuration.AppSettings.ServerDirectoryPath))
        {
            AutoDetectPzServer();
        }
    }

    private string FindConfigDirectory(string rootPath)
    {
        // 1. Check direct config in root
        var direct = Path.Combine(rootPath, "config");
        if (Directory.Exists(direct)) return direct;

        // 2. Check current directory
        var current = Path.Combine(Directory.GetCurrentDirectory(), "config");
        if (Directory.Exists(current)) return current;

        // 3. Walk up
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
        _logger.LogInformation("Starting auto-detection of Project Zomboid server files...");

        // Common paths in containers and typical installations
        var searchPaths = new List<string>
        {
            "/project-zomboid-config/Server", // Docker indifferentbroccoli
			"/home/steam/Zomboid/Server",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Zomboid", "Server")
        };

        // If in container, scan root mounts
        if (IsRunningInContainer())
        {
            _logger.LogInformation("Detected container environment. Scanning common mounts...");
            searchPaths.Add("/project-zomboid-config");
            searchPaths.Add("/data/Server");
        }

        foreach (var path in searchPaths)
        {
            if (Directory.Exists(path))
            {
                var iniFiles = Directory.GetFiles(path, "*.ini");
                if (iniFiles.Length > 0)
                {
                    // Prefer servertest.ini or the first one found
                    var iniFile = iniFiles.FirstOrDefault(f => f.EndsWith("servertest.ini")) ?? iniFiles[0];
                    var serverName = Path.GetFileNameWithoutExtension(iniFile);

                    _logger.LogInformation("Found PZ server configuration at {Path}. Server name: {ServerName}", path, serverName);

                    _configuration.AppSettings.ServerDirectoryPath = path;
                    _configuration.AppSettings.ActiveServer = serverName;

                    SaveConfiguration(_configuration);
                    return;
                }
            }
        }

        _logger.LogWarning("Could not auto-detect Project Zomboid server files.");
    }

    private bool IsRunningInContainer()
    {
        return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
            || File.Exists("/.dockerenv");
    }

    /// <summary>
    /// Gets the current configuration
    /// </summary>
    /// <returns>Current ZSM configuration</returns>
    public ZsmConfiguration GetConfiguration()
    {
        lock (_lock)
        {
            return _configuration;
        }
    }

    /// <summary>
    /// Saves configuration to file
    /// </summary>
    /// <param name="configuration">Configuration to save</param>
    public async Task SaveConfigurationAsync(ZsmConfiguration configuration)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(configuration, options);
            await File.WriteAllTextAsync(_configFilePath, json);

            lock (_lock)
            {
                _configuration = configuration;
            }

            _logger.LogInformation("Configuration saved successfully to {FilePath}", _configFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save configuration to {FilePath}", _configFilePath);
            throw;
        }
    }

    /// <summary>
    /// Saves configuration to file (synchronous)
    /// </summary>
    /// <param name="configuration">Configuration to save</param>
    public void SaveConfiguration(ZsmConfiguration configuration)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(configuration, options);
            File.WriteAllText(_configFilePath, json);

            lock (_lock)
            {
                _configuration = configuration;
            }

            _logger.LogInformation("Configuration saved successfully to {FilePath}", _configFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save configuration to {FilePath}", _configFilePath);
            throw;
        }
    }

    /// <summary>
    /// Reloads configuration from file
    /// </summary>
    public async Task ReloadConfigurationAsync()
    {
        await Task.Run(LoadConfiguration);
    }

    /// <summary>
    /// Loads configuration from file
    /// </summary>
    private void LoadConfiguration()
    {
        try
        {
            if (!File.Exists(_configFilePath))
            {
                _logger.LogWarning("Configuration file not found at {FilePath}. Using default configuration.", _configFilePath);
                CreateDefaultConfiguration();
                return;
            }

            var json = File.ReadAllText(_configFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            var configuration = JsonSerializer.Deserialize<ZsmConfiguration>(json, options);

            lock (_lock)
            {
                _configuration = configuration ?? new ZsmConfiguration();
            }

            _logger.LogInformation("Configuration loaded successfully from {FilePath}", _configFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load configuration from {FilePath}. Using default configuration.", _configFilePath);
            CreateDefaultConfiguration();
        }
    }

    /// <summary>
    /// Creates a default configuration
    /// </summary>
    private void CreateDefaultConfiguration()
    {
        lock (_lock)
        {
            _configuration = new ZsmConfiguration
            {
                AppSettings = new AppSettings
                {
                    ServerDirectoryPath = "",
                    ActiveServer = "",
                    RCON = new RconSettings()
                },
                Users = new List<AuthUser>(),
                Roles = new Dictionary<string, RolePermissions>
                {
                    ["Guest"] = new RolePermissions
                    {
                        AllowConfigEdit = false,
                        AllowRcon = false,
                        AllowModManagement = false,
                        AllowDatabaseWrite = false,
                        AllowServerSwitch = false
                    },
                    ["Moderator"] = new RolePermissions
                    {
                        AllowConfigEdit = false,
                        AllowRcon = true,
                        AllowModManagement = false,
                        AllowDatabaseWrite = false,
                        AllowServerSwitch = false
                    },
                    ["Administrator"] = new RolePermissions
                    {
                        AllowConfigEdit = true,
                        AllowRcon = true,
                        AllowModManagement = true,
                        AllowDatabaseWrite = true,
                        AllowServerSwitch = true
                    }
                }
            };
        }
    }
}
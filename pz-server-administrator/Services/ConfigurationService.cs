using System.Text.Json;
using pz_server_administrator.Models;

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
    public ConfigurationService(ILogger<ConfigurationService> logger)
    {
        _logger = logger;
        _configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "config", "appsettings.zsm.json");
        _configuration = new ZsmConfiguration();
        
        // Load configuration on startup
        LoadConfiguration();
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
                Users = new List<User>(),
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
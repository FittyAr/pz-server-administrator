using pz_server_administrator.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;

namespace pz_server_administrator.Services;

/// <summary>
/// Service for managing application configuration from appsettings.json
/// </summary>
public interface IConfigurationService
{
    ZsmConfiguration GetConfiguration();
    AppSettings GetAppSettings();
    Task SaveConfigurationAsync(ZsmConfiguration configuration);
    Task SaveAppSettingsAsync(AppSettings settings);
    void SaveConfiguration(ZsmConfiguration configuration);
    Task ReloadConfigurationAsync();
    bool RunDeepScan(string rootPath);
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

        // Determine persistent config directory:
        // Priority 1: PZ_ADMIN_CONFIG_DIR or CONFIG_DIR
        // Priority 2: /app/config if running in container / existing
        // Priority 3: env.ContentRootPath/Resources
        string? configDir = Environment.GetEnvironmentVariable("PZ_ADMIN_CONFIG_DIR")
            ?? Environment.GetEnvironmentVariable("CONFIG_DIR");

        if (string.IsNullOrWhiteSpace(configDir))
        {
            var appConfigDir = Path.Combine(env.ContentRootPath, "config");
            if (Directory.Exists(appConfigDir) || Directory.Exists("/app/config"))
            {
                configDir = Directory.Exists(appConfigDir) ? appConfigDir : "/app/config";
            }
            else
            {
                configDir = Path.Combine(env.ContentRootPath, "Resources");
            }
        }

        if (!Directory.Exists(configDir))
        {
            try { Directory.CreateDirectory(configDir); } catch { }
        }

        var customPath = Environment.GetEnvironmentVariable("PZ_CONFIG_PATH");
        _configFilePath = !string.IsNullOrWhiteSpace(customPath)
            ? customPath
            : Path.Combine(configDir, "config.json");

        // If target config.json does not exist yet, copy from template in Resources
        var templateConfigPath = Path.Combine(env.ContentRootPath, "Resources", "config.json");
        if (!File.Exists(_configFilePath) && File.Exists(templateConfigPath))
        {
            try
            {
                File.Copy(templateConfigPath, _configFilePath);
                _logger.LogInformation("[Configuration] Initialized config file from template at {Path}", _configFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[Configuration] Failed to copy template config: {Msg}", ex.Message);
            }
        }

        _configuration = new ZsmConfiguration();
        _logger.LogInformation("[Configuration] Initializing with config file: {FilePath}", _configFilePath);

        LoadConfiguration();

        // Apply environment variable overrides (e.g. from docker-compose / .env)
        ApplyEnvironmentOverrides();

        if (string.IsNullOrEmpty(_configuration.AppSettings.ServerDirectoryPath) || !Directory.Exists(_configuration.AppSettings.ServerDirectoryPath))
        {
            AutoDetectPzServer();
        }
        else if (string.IsNullOrEmpty(_configuration.AppSettings.PlayersDatabasePath) || !File.Exists(_configuration.AppSettings.PlayersDatabasePath))
        {
            _logger.LogInformation("[Configuration] Valid ServerDirectoryPath found but missing DB Paths. Running Deep Scan automatically.");
            var rootDir = new DirectoryInfo(_configuration.AppSettings.ServerDirectoryPath).Parent?.FullName ?? _configuration.AppSettings.ServerDirectoryPath;
            RunDeepScan(rootDir);
        }
    }

    private void ApplyEnvironmentOverrides()
    {
        var appSettings = _configuration.AppSettings;

        var serverName = Environment.GetEnvironmentVariable("PZ_ACTIVE_SERVER")
            ?? Environment.GetEnvironmentVariable("SERVER_NAME");
        if (!string.IsNullOrWhiteSpace(serverName))
        {
            appSettings.ActiveServer = serverName.Trim();
        }

        var serverDir = Environment.GetEnvironmentVariable("PZ_SERVER_DIR");
        if (!string.IsNullOrWhiteSpace(serverDir))
        {
            appSettings.ServerDirectoryPath = serverDir.Trim();
        }

        var zomboidDir = Environment.GetEnvironmentVariable("PZ_ZOMBOID_DIR");
        if (!string.IsNullOrWhiteSpace(zomboidDir))
        {
            appSettings.ZomboidDirectory = zomboidDir.Trim();
        }

        var rconHost = Environment.GetEnvironmentVariable("PZ_RCON_HOST")
            ?? Environment.GetEnvironmentVariable("RCON_HOST");
        if (!string.IsNullOrWhiteSpace(rconHost))
        {
            appSettings.Rcon.Host = rconHost.Trim();
        }

        var rconPortStr = Environment.GetEnvironmentVariable("PZ_RCON_PORT")
            ?? Environment.GetEnvironmentVariable("RCON_PORT");
        if (int.TryParse(rconPortStr, out var rconPort) && rconPort > 0)
        {
            appSettings.Rcon.Port = rconPort;
        }

        var rconPass = Environment.GetEnvironmentVariable("PZ_RCON_PASSWORD")
            ?? Environment.GetEnvironmentVariable("RCON_PASSWORD");
        if (!string.IsNullOrWhiteSpace(rconPass))
        {
            appSettings.Rcon.Password = rconPass.Trim();
        }

        var iniPath = Environment.GetEnvironmentVariable("PZ_INI_PATH");
        if (!string.IsNullOrWhiteSpace(iniPath))
        {
            appSettings.ServerIniPath = iniPath.Trim();
        }

        var sandboxPath = Environment.GetEnvironmentVariable("PZ_SANDBOX_PATH");
        if (!string.IsNullOrWhiteSpace(sandboxPath))
        {
            appSettings.SandboxVarsPath = sandboxPath.Trim();
        }

        var spawnRegionsPath = Environment.GetEnvironmentVariable("PZ_SPAWNREGIONS_PATH");
        if (!string.IsNullOrWhiteSpace(spawnRegionsPath))
        {
            appSettings.SpawnRegionsPath = spawnRegionsPath.Trim();
        }

        var playersDb = Environment.GetEnvironmentVariable("PZ_PLAYERS_DB");
        if (!string.IsNullOrWhiteSpace(playersDb))
        {
            appSettings.PlayersDatabasePath = playersDb.Trim();
        }

        var vehiclesDb = Environment.GetEnvironmentVariable("PZ_VEHICLES_DB");
        if (!string.IsNullOrWhiteSpace(vehiclesDb))
        {
            appSettings.VehiclesDatabasePath = vehiclesDb.Trim();
        }

        var serverTestDb = Environment.GetEnvironmentVariable("PZ_SERVERTEST_DB");
        if (!string.IsNullOrWhiteSpace(serverTestDb))
        {
            appSettings.ServerTestDatabasePath = serverTestDb.Trim();
        }

        var workshopDir = Environment.GetEnvironmentVariable("PZ_WORKSHOP_DIR");
        if (!string.IsNullOrWhiteSpace(workshopDir))
        {
            appSettings.WorkshopDirectoryPath = workshopDir.Trim();
        }

        // Check if admin user/password was provided via environment
        var adminUser = Environment.GetEnvironmentVariable("PZ_ADMIN_USERNAME")
            ?? Environment.GetEnvironmentVariable("ADMIN_USERNAME");
        var adminPass = Environment.GetEnvironmentVariable("PZ_ADMIN_PASSWORD")
            ?? Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        if (!string.IsNullOrWhiteSpace(adminUser) && !string.IsNullOrWhiteSpace(adminPass) && adminPass != "CHANGEME")
        {
            var existingAdmin = _configuration.Users.FirstOrDefault(u => u.Username.Equals(adminUser.Trim(), StringComparison.OrdinalIgnoreCase));
            var hashed = BCrypt.Net.BCrypt.HashPassword(adminPass.Trim(), 12);
            if (existingAdmin != null)
            {
                existingAdmin.PasswordHash = hashed;
            }
            else
            {
                _configuration.Users.Add(new AuthUser
                {
                    Username = adminUser.Trim(),
                    PasswordHash = hashed,
                    Role = UserRole.Administrator,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
    }

    private void AutoDetectPzServer()
    {
        _logger.LogInformation("[AutoDetect] Starting Project Zomboid server file discovery...");

        var potentialPaths = new List<string>
        {
            "/project-zomboid-config/Server",
            "/project-zomboid-config",
            "/data",
            "/home/steam/Zomboid",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Zomboid"),
            "./server-data"
        };

        if (IsRunningInContainer())
        {
            _logger.LogInformation("[AutoDetect] Container environment detected.");
        }

        foreach (var path in potentialPaths)
        {
            _logger.LogDebug("[AutoDetect] Checking path: {Path}", path);
            if (RunDeepScan(path)) return; // If successful deep scan finds something, stop
        }

        _logger.LogWarning("[AutoDetect] Could not find any Project Zomboid server configuration.");
    }

    public bool RunDeepScan(string rootPath)
    {
        if (!Directory.Exists(rootPath)) return false;

        _logger.LogInformation("[DeepScan] Starting deep scan at {Path}", rootPath);

        string iniPath = "";
        string luaPath = "";
        string spawnRegionsPath = "";
        string playersDb = "";
        string vehiclesDb = "";
        string serverTestDb = "";
        string expectedPrefix = _configuration.AppSettings.ActiveServer;
        if (string.IsNullOrEmpty(expectedPrefix)) expectedPrefix = "pzserver";

        SafeDeepScan(rootPath, expectedPrefix, ref iniPath, ref luaPath, ref spawnRegionsPath, ref playersDb, ref vehiclesDb, ref serverTestDb, 0);

        bool foundAnything = false;

        lock (_lock)
        {
            if (!string.IsNullOrEmpty(iniPath))
            {
                _configuration.AppSettings.ServerDirectoryPath = Path.GetDirectoryName(iniPath) ?? "";
                _configuration.AppSettings.ServerIniPath = iniPath;
                _configuration.AppSettings.ActiveServer = Path.GetFileNameWithoutExtension(iniPath);
                foundAnything = true;
            }

            if (!string.IsNullOrEmpty(luaPath)) { _configuration.AppSettings.SandboxVarsPath = luaPath; foundAnything = true; }
            if (!string.IsNullOrEmpty(spawnRegionsPath)) { _configuration.AppSettings.SpawnRegionsPath = spawnRegionsPath; foundAnything = true; }
            if (!string.IsNullOrEmpty(playersDb)) { _configuration.AppSettings.PlayersDatabasePath = playersDb; foundAnything = true; }
            if (!string.IsNullOrEmpty(vehiclesDb)) { _configuration.AppSettings.VehiclesDatabasePath = vehiclesDb; foundAnything = true; }
            if (!string.IsNullOrEmpty(serverTestDb)) { _configuration.AppSettings.ServerTestDatabasePath = serverTestDb; foundAnything = true; }

            // Set ZomboidDirectory if not explicitly set
            if (string.IsNullOrEmpty(_configuration.AppSettings.ZomboidDirectory))
            {
                if (!string.IsNullOrEmpty(_configuration.AppSettings.ServerDirectoryPath) &&
                    _configuration.AppSettings.ServerDirectoryPath.EndsWith("Server", StringComparison.OrdinalIgnoreCase))
                {
                    _configuration.AppSettings.ZomboidDirectory = Directory.GetParent(_configuration.AppSettings.ServerDirectoryPath)?.FullName ?? _configuration.AppSettings.ServerDirectoryPath;
                }
                else
                {
                    _configuration.AppSettings.ZomboidDirectory = rootPath;
                }
            }
        }

        if (foundAnything)
        {
            SaveConfiguration(_configuration);
            _logger.LogInformation("[DeepScan] Scan complete. Found INI: {HasIni}, DBs: {HasDbs}", !string.IsNullOrEmpty(iniPath), !string.IsNullOrEmpty(playersDb) || !string.IsNullOrEmpty(serverTestDb));
        }

        return foundAnything;
    }

    private void SafeDeepScan(string rootPath, string expectedPrefix, ref string iniPath, ref string luaPath, ref string spawnRegionsPath, ref string playersDb, ref string vehiclesDb, ref string serverTestDb, int depth)
    {
        if (depth > 5) return; // limit depth to avoid excessive nesting

        try
        {
            foreach (var file in Directory.GetFiles(rootPath))
            {
                var fileName = Path.GetFileName(file);

                // INI
                if (string.Equals(fileName, $"{expectedPrefix}.ini", StringComparison.OrdinalIgnoreCase)) iniPath = file;
                else if (string.IsNullOrEmpty(iniPath) && fileName.EndsWith(".ini", StringComparison.OrdinalIgnoreCase)) iniPath = file;

                // SandboxVars.lua
                else if (string.Equals(fileName, $"{expectedPrefix}_SandboxVars.lua", StringComparison.OrdinalIgnoreCase)) luaPath = file;
                else if (string.IsNullOrEmpty(luaPath) && fileName.EndsWith("_SandboxVars.lua", StringComparison.OrdinalIgnoreCase)) luaPath = file;

                // SpawnRegions.lua
                else if (string.Equals(fileName, $"{expectedPrefix}_spawnregions.lua", StringComparison.OrdinalIgnoreCase)) spawnRegionsPath = file;
                else if (string.IsNullOrEmpty(spawnRegionsPath) && fileName.EndsWith("_spawnregions.lua", StringComparison.OrdinalIgnoreCase)) spawnRegionsPath = file;

                // Players & Vehicles DB
                else if (string.Equals(fileName, "players.db", StringComparison.OrdinalIgnoreCase)) playersDb = file;
                else if (string.Equals(fileName, "vehicles.db", StringComparison.OrdinalIgnoreCase)) vehiclesDb = file;

                // Main Server Database (e.g. pzserver.sqlite, servertest.sqlite, or .db)
                else if (string.Equals(fileName, $"{expectedPrefix}.sqlite", StringComparison.OrdinalIgnoreCase) || string.Equals(fileName, $"{expectedPrefix}.db", StringComparison.OrdinalIgnoreCase)) serverTestDb = file;
                else if (string.IsNullOrEmpty(serverTestDb) && (fileName.EndsWith(".sqlite", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
                         && !string.Equals(fileName, "players.db", StringComparison.OrdinalIgnoreCase) && !string.Equals(fileName, "vehicles.db", StringComparison.OrdinalIgnoreCase)) serverTestDb = file;
            }

            foreach (var dir in Directory.GetDirectories(rootPath))
            {
                SafeDeepScan(dir, expectedPrefix, ref iniPath, ref luaPath, ref spawnRegionsPath, ref playersDb, ref vehiclesDb, ref serverTestDb, depth + 1);
            }
        }
        catch (UnauthorizedAccessException) { }
        catch (DirectoryNotFoundException) { }
        catch (Exception ex)
        {
            _logger.LogTrace("[DeepScan] Minor error at {Path}: {Msg}", rootPath, ex.Message);
        }
    }

    private bool IsRunningInContainer()
    {
        var isContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
            || File.Exists("/.dockerenv")
            || Environment.GetEnvironmentVariable("container") == "podman";

        return isContainer;
    }

    public ZsmConfiguration GetConfiguration() => _configuration;

    public AppSettings GetAppSettings() => _configuration.AppSettings;

    public async Task SaveConfigurationAsync(ZsmConfiguration configuration)
    {
        lock (_lock) _configuration = configuration;
        await SaveConfigurationToFileAsync();
    }

    public async Task SaveAppSettingsAsync(AppSettings settings)
    {
        lock (_lock) _configuration.AppSettings = settings;
        await SaveConfigurationToFileAsync();
    }

    private async Task SaveConfigurationToFileAsync()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            var existingJson = File.Exists(_configFilePath) ? await File.ReadAllTextAsync(_configFilePath) : "{}";
            var jsonObj = System.Text.Json.Nodes.JsonNode.Parse(existingJson) as System.Text.Json.Nodes.JsonObject ?? new System.Text.Json.Nodes.JsonObject();

            var configNode = JsonSerializer.SerializeToNode(_configuration, options) as System.Text.Json.Nodes.JsonObject;
            if (configNode != null)
            {
                foreach (var prop in configNode)
                {
                    if (prop.Value == null)
                        jsonObj[prop.Key] = null;
                    else
                        jsonObj[prop.Key] = prop.Value.DeepClone();
                }
            }

            var newJsonString = jsonObj.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_configFilePath, newJsonString);

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

            var existingJson = File.Exists(_configFilePath) ? File.ReadAllText(_configFilePath) : "{}";
            var jsonObj = System.Text.Json.Nodes.JsonNode.Parse(existingJson) as System.Text.Json.Nodes.JsonObject ?? new System.Text.Json.Nodes.JsonObject();

            var configNode = JsonSerializer.SerializeToNode(configuration, options) as System.Text.Json.Nodes.JsonObject;
            if (configNode != null)
            {
                foreach (var prop in configNode)
                {
                    if (prop.Value == null)
                        jsonObj[prop.Key] = null;
                    else
                        jsonObj[prop.Key] = prop.Value.DeepClone();
                }
            }

            var newJsonString = jsonObj.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configFilePath, newJsonString);

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
                AppSettings = new AppSettings
                {
                    ServerDirectoryPath = "/project-zomboid-config/Server",
                    ActiveServer = "pzserver",
                    ZomboidDirectory = "/project-zomboid-config",
                    ServerIniPath = "/project-zomboid-config/Server/pzserver.ini",
                    SandboxVarsPath = "/project-zomboid-config/Server/pzserver_SandboxVars.lua",
                    SpawnRegionsPath = "/project-zomboid-config/Server/pzserver_spawnregions.lua",
                    PlayersDatabasePath = "/project-zomboid-config/Saves/Multiplayer/pzserver/players.db",
                    VehiclesDatabasePath = "/project-zomboid-config/Saves/Multiplayer/pzserver/vehicles.db",
                    ServerTestDatabasePath = "/project-zomboid-config/db/pzserver.sqlite",
                    Rcon = new RconSettings
                    {
                        Host = "projectzomboid",
                        Port = 27015,
                        Password = ""
                    },
                    Language = "es"
                },
                Users = new List<AuthUser>
                {
                    new AuthUser
                    {
                        Username = "admin",
                        PasswordHash = "$2a$12$vEi2UHPdOFIl5cQYiucrz.082wgWl.8/wJc0Cs0FEZzxW9b7i3cl6",
                        Role = UserRole.Administrator,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                },
                Roles = new Dictionary<string, RolePermissions>
                {
                    ["Guest"] = new RolePermissions { AllowConfigEdit = false, AllowRcon = false, AllowModManagement = false, AllowDatabaseWrite = false, AllowServerSwitch = false, AllowUserManagement = false },
                    ["Moderator"] = new RolePermissions { AllowConfigEdit = false, AllowRcon = true, AllowModManagement = true, AllowDatabaseWrite = true, AllowServerSwitch = false, AllowUserManagement = false },
                    ["Administrator"] = new RolePermissions { AllowConfigEdit = true, AllowRcon = true, AllowModManagement = true, AllowDatabaseWrite = true, AllowServerSwitch = true, AllowUserManagement = true }
                }
            };
        }
    }
}
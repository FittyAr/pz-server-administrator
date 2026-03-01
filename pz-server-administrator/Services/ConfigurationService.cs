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

        // El usuario solicitó centralizar toda la configuración persistente en la carpeta /Resources
        // Esto permite mapear un volumen de Docker a /app/Resources para persistencia total.
        var resourcesDir = Path.Combine(env.ContentRootPath, "Resources");
        if (!Directory.Exists(resourcesDir))
        {
            Directory.CreateDirectory(resourcesDir);
            _logger.LogInformation("[Configuration] Created Resources directory at {Path}", resourcesDir);
        }

        _configFilePath = Path.Combine(resourcesDir, "config.json");

        // Si el archivo no existe, intentamos migrar desde el appsettings.json de la raíz si existe
        var legacyConfigPath = Path.Combine(env.ContentRootPath, "appsettings.json");
        if (!File.Exists(_configFilePath) && File.Exists(legacyConfigPath))
        {
            try
            {
                File.Copy(legacyConfigPath, _configFilePath);
                _logger.LogInformation("[Configuration] Migrated legacy appsettings.json to {Path}", _configFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[Configuration] Failed to migrate legacy config: {Msg}", ex.Message);
            }
        }
        _configuration = new ZsmConfiguration();

        _logger.LogInformation("[Configuration] Initializing with config file: {FilePath}", _configFilePath);

        LoadConfiguration();

        if (string.IsNullOrEmpty(_configuration.AppSettings.ServerDirectoryPath) || !Directory.Exists(_configuration.AppSettings.ServerDirectoryPath))
        {
            AutoDetectPzServer();
        }
        else if (string.IsNullOrEmpty(_configuration.AppSettings.PlayersDatabasePath) || !File.Exists(_configuration.AppSettings.PlayersDatabasePath))
        {
            // Si la ruta base está configurada pero no encuentra DBs, escanee de forma local
            _logger.LogInformation("[Configuration] Valid ServerDirectoryPath found but missing DB Paths. Running Deep Scan automatically.");
            var rootDir = new DirectoryInfo(_configuration.AppSettings.ServerDirectoryPath).Parent?.FullName ?? _configuration.AppSettings.ServerDirectoryPath;
            RunDeepScan(rootDir);
        }
    }



    private void AutoDetectPzServer()
    {
        _logger.LogInformation("[AutoDetect] Starting Project Zomboid server file discovery...");

        var potentialPaths = new List<string>
        {
            "/project-zomboid-config",
            "/data",
            "/home/steam/Zomboid",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Zomboid")
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
        string playersDb = "";
        string vehiclesDb = "";
        string serverTestDb = "";
        string expectedPrefix = _configuration.AppSettings.ActiveServer;
        if (string.IsNullOrEmpty(expectedPrefix)) expectedPrefix = "servertest";

        SafeDeepScan(rootPath, expectedPrefix, ref iniPath, ref luaPath, ref playersDb, ref vehiclesDb, ref serverTestDb, 0);

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
            if (!string.IsNullOrEmpty(playersDb)) { _configuration.AppSettings.PlayersDatabasePath = playersDb; foundAnything = true; }
            if (!string.IsNullOrEmpty(vehiclesDb)) { _configuration.AppSettings.VehiclesDatabasePath = vehiclesDb; foundAnything = true; }
            if (!string.IsNullOrEmpty(serverTestDb)) { _configuration.AppSettings.ServerTestDatabasePath = serverTestDb; foundAnything = true; }
        }

        if (foundAnything)
        {
            SaveConfiguration(_configuration);
            _logger.LogInformation("[DeepScan] Scan complete. Found INI: {HasIni}, DBs: {HasDbs}", !string.IsNullOrEmpty(iniPath), !string.IsNullOrEmpty(playersDb));
        }

        return foundAnything;
    }

    private void SafeDeepScan(string rootPath, string expectedPrefix, ref string iniPath, ref string luaPath, ref string playersDb, ref string vehiclesDb, ref string serverTestDb, int depth)
    {
        if (depth > 5) return; // limit depth to avoid excessive nesting

        try
        {
            foreach (var file in Directory.GetFiles(rootPath))
            {
                var fileName = Path.GetFileName(file);

                // Si aún no tenemos iniPath y encontramos uno .ini, lo capturamos. Idealmente buscamos el expectedPrefix
                if (string.Equals(fileName, $"{expectedPrefix}.ini", StringComparison.OrdinalIgnoreCase)) iniPath = file;
                else if (string.IsNullOrEmpty(iniPath) && fileName.EndsWith(".ini", StringComparison.OrdinalIgnoreCase)) iniPath = file;

                else if (string.Equals(fileName, $"{expectedPrefix}_SandboxVars.lua", StringComparison.OrdinalIgnoreCase)) luaPath = file;
                else if (string.IsNullOrEmpty(luaPath) && fileName.EndsWith("_SandboxVars.lua", StringComparison.OrdinalIgnoreCase)) luaPath = file;

                else if (string.Equals(fileName, "players.db", StringComparison.OrdinalIgnoreCase)) playersDb = file;
                else if (string.Equals(fileName, "vehicles.db", StringComparison.OrdinalIgnoreCase)) vehiclesDb = file;

                else if (string.Equals(fileName, $"{expectedPrefix}.db", StringComparison.OrdinalIgnoreCase)) serverTestDb = file;
                else if (string.IsNullOrEmpty(serverTestDb) && fileName.EndsWith(".db", StringComparison.OrdinalIgnoreCase) && !string.Equals(fileName, "players.db", StringComparison.OrdinalIgnoreCase) && !string.Equals(fileName, "vehicles.db", StringComparison.OrdinalIgnoreCase)) serverTestDb = file;
            }

            foreach (var dir in Directory.GetDirectories(rootPath))
            {
                SafeDeepScan(dir, expectedPrefix, ref iniPath, ref luaPath, ref playersDb, ref vehiclesDb, ref serverTestDb, depth + 1);
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
                AppSettings = new AppSettings { ServerDirectoryPath = "", ActiveServer = "", Rcon = new RconSettings() },
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
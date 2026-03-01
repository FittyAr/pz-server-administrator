using System.Text;
using System.Text.RegularExpressions;
using pz_server_administrator.Models;

namespace pz_server_administrator.Services;

public class PzServerService : IPzServerService
{
    private readonly ILogger<PzServerService> _logger;
    private readonly IConfigurationService _configService;

    public PzServerService(ILogger<PzServerService> logger, IConfigurationService configService)
    {
        _logger = logger;
        _configService = configService;
    }

    public async Task<List<PzFile>> GetServerFilesAsync(string serverPath)
    {
        if (string.IsNullOrWhiteSpace(serverPath) || !Directory.Exists(serverPath))
        {
            return new List<PzFile>();
        }

        return await Task.Run(() =>
        {
            var files = new List<PzFile>();
            var directoryInfo = new DirectoryInfo(serverPath);

            foreach (var file in directoryInfo.GetFiles("*.*", SearchOption.AllDirectories))
            {
                var type = file.Extension.ToLower() switch
                {
                    ".ini" => PzFileType.Ini,
                    ".lua" => PzFileType.Lua,
                    ".db" or ".sqlite" => PzFileType.Database,
                    _ => PzFileType.Other
                };

                if (type != PzFileType.Other)
                {
                    files.Add(new PzFile
                    {
                        Name = file.Name,
                        FullPath = file.FullName,
                        Type = type,
                        SizeBytes = file.Length,
                        LastModified = file.LastWriteTime
                    });
                }
            }

            return files.OrderBy(f => f.Name).ToList();
        });
    }

    public async Task<string> ReadFileContentAsync(string filePath)
    {
        if (!File.Exists(filePath)) return string.Empty;
        return await File.ReadAllTextAsync(filePath);
    }

    public async Task SaveFileContentAsync(string filePath, string content)
    {
        await File.WriteAllTextAsync(filePath, content);
    }

    public async Task<PzConfig> ParseConfigAsync(string filePath)
    {
        var config = new PzConfig { FilePath = filePath };
        if (!File.Exists(filePath)) return config;

        var content = await File.ReadAllTextAsync(filePath);
        var extension = Path.GetExtension(filePath).ToLower();

        if (extension == ".ini")
        {
            ParseIni(content, config);
        }
        else if (extension == ".lua")
        {
            ParseLua(content, config);
        }

        return config;
    }

    public async Task SaveConfigAsync(PzConfig config)
    {
        var extension = Path.GetExtension(config.FilePath).ToLower();
        string content = "";

        if (extension == ".ini")
        {
            content = SerializeIni(config);
        }
        else if (extension == ".lua")
        {
            content = SerializeLua(config);
        }

        if (!string.IsNullOrEmpty(content))
        {
            await File.WriteAllTextAsync(config.FilePath, content);
        }
    }

    private void ParseIni(string content, PzConfig config)
    {
        var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string lastComment = string.Empty;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                lastComment = string.Empty;
                continue;
            }

            if (trimmed.StartsWith("#") || trimmed.StartsWith(";"))
            {
                lastComment = trimmed.TrimStart('#', ';', ' ').Trim();
                continue;
            }

            var parts = trimmed.Split('=', 2);
            if (parts.Length == 2)
            {
                config.Entries.Add(new PzConfigEntry
                {
                    Key = parts[0].Trim(),
                    Value = parts[1].Trim(),
                    Description = lastComment,
                    Type = DetectType(parts[1].Trim())
                });
                lastComment = string.Empty;
            }
        }
    }

    private string SerializeIni(PzConfig config)
    {
        var sb = new StringBuilder();
        foreach (var entry in config.Entries)
        {
            sb.AppendLine($"{entry.Key}={entry.Value}");
        }
        return sb.ToString();
    }

    private void ParseLua(string content, PzConfig config)
    {
        var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string lastComment = string.Empty;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Skip grouping blocks like SandboxVars = {
            if (trimmed.EndsWith("{") || trimmed.EndsWith("}")) continue;

            if (trimmed.StartsWith("--"))
            {
                lastComment = trimmed.TrimStart('-', ' ').Trim();
                continue;
            }

            // Match Key = Value,
            var match = Regex.Match(trimmed, @"(\w+)\s*=\s*([^,]+),?");
            if (match.Success)
            {
                config.Entries.Add(new PzConfigEntry
                {
                    Key = match.Groups[1].Value,
                    Value = match.Groups[2].Value.Trim().Trim('"').Trim('\''),
                    Description = lastComment,
                    Type = DetectType(match.Groups[2].Value.Trim())
                });
                lastComment = string.Empty;
            }
        }
    }

    private string SerializeLua(PzConfig config)
    {
        // This is a simplified LUA serializer for SandboxVars
        var sb = new StringBuilder();
        sb.AppendLine("SandboxVars = {");
        foreach (var entry in config.Entries)
        {
            var value = entry.Type == "String" ? $"\"{entry.Value}\"" : entry.Value;
            sb.AppendLine($"    {entry.Key} = {value},");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    public async Task<List<SpawnRegion>> ParseSpawnRegionsAsync(string filePath)
    {
        var regions = new List<SpawnRegion>();
        if (!File.Exists(filePath)) return regions;

        var content = await File.ReadAllTextAsync(filePath);
        // Match { name = "...", file = "..." } or { name = "...", serverfile = "..." }
        var regex = new Regex(@"\{\s*name\s*=\s*""([^""]+)"",\s*(file|serverfile)\s*=\s*""([^""]+)""\s*\}");
        var matches = regex.Matches(content);

        foreach (Match match in matches)
        {
            var region = new SpawnRegion { Name = match.Groups[1].Value };
            if (match.Groups[2].Value == "file") region.File = match.Groups[3].Value;
            else region.ServerFile = match.Groups[3].Value;
            regions.Add(region);
        }

        return regions;
    }

    public async Task<(int port, string password)> GetRconCredentialsAsync(string serverPath, string activeServer)
    {
        try
        {
            var filePath = Path.Combine(serverPath, $"{activeServer}.ini");
            if (!File.Exists(filePath)) return (27015, string.Empty);

            var config = await ParseConfigAsync(filePath);

            var rconPortEntry = config.Entries.FirstOrDefault(e => e.Key.Equals("RCONPort", StringComparison.OrdinalIgnoreCase));
            var rconPassEntry = config.Entries.FirstOrDefault(e => e.Key.Equals("RCONPassword", StringComparison.OrdinalIgnoreCase));

            int port = 27015; // default PZ RCON port
            if (rconPortEntry != null && int.TryParse(rconPortEntry.Value, out int parsedPort))
            {
                port = parsedPort;
            }

            string password = rconPassEntry?.Value ?? string.Empty;

            return (port, password);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PzServerService] Error getting RCON credentials: {ex.Message}");
            return (27015, string.Empty);
        }
    }

    public async Task SaveSpawnRegionsAsync(string filePath, List<SpawnRegion> regions)
    {
        var sb = new StringBuilder();
        sb.AppendLine("function SpawnRegions()");
        sb.AppendLine("        return {");
        foreach (var region in regions)
        {
            var fileType = !string.IsNullOrEmpty(region.File) ? "file" : "serverfile";
            var fileName = region.File ?? region.ServerFile;
            sb.AppendLine($"                {{ name = \"{region.Name}\", {fileType} = \"{fileName}\" }},");
        }
        sb.AppendLine("        }");
        sb.AppendLine("end");
        await File.WriteAllTextAsync(filePath, sb.ToString());
    }

    public async Task<SpawnPointsConfig> ParseSpawnPointsAsync(string filePath)
    {
        var config = new SpawnPointsConfig { FilePath = filePath };
        if (!File.Exists(filePath)) return config;

        var content = await File.ReadAllTextAsync(filePath);

        // Improved regex to handle nested braces (profession = { {点}, {点} })
        // This matches key = { ... } where ... can contain nested braces
        var profRegex = new Regex(@"(\w+)\s*=\s*\{([\s\S]*?)\n\s*\},?", RegexOptions.Multiline);
        var profMatches = profRegex.Matches(content);

        foreach (Match profMatch in profMatches)
        {
            var profession = profMatch.Groups[1].Value;
            var pointsContent = profMatch.Groups[2].Value;

            // Skip if it's just the function name or something else
            if (profession == "function" || profession == "return") continue;

            var points = new List<SpawnPoint>();
            // Match: { worldX = ..., worldY = ..., posX = ..., posY = ... }
            var pointRegex = new Regex(@"\{\s*worldX\s*=\s*(\d+),\s*worldY\s*=\s*(\d+),\s*posX\s*=\s*(\d+),\s*posY\s*=\s*(\d+)(?:,\s*posZ\s*=\s*(\d+))?\s*\}");
            var pointMatches = pointRegex.Matches(pointsContent);

            foreach (Match pMatch in pointMatches)
            {
                points.Add(new SpawnPoint
                {
                    WorldX = int.Parse(pMatch.Groups[1].Value),
                    WorldY = int.Parse(pMatch.Groups[2].Value),
                    PosX = int.Parse(pMatch.Groups[3].Value),
                    PosY = int.Parse(pMatch.Groups[4].Value),
                    PosZ = pMatch.Groups[5].Success ? int.Parse(pMatch.Groups[5].Value) : 0
                });
            }

            if (points.Count > 0)
            {
                config.ProfessionPoints[profession] = points;
            }
        }

        return config;
    }

    public async Task SaveSpawnPointsAsync(SpawnPointsConfig config)
    {
        var sb = new StringBuilder();
        sb.AppendLine("function SpawnPoints()");
        sb.AppendLine("        return {");
        foreach (var prof in config.ProfessionPoints)
        {
            sb.AppendLine($"                {prof.Key} = {{");
            foreach (var point in prof.Value)
            {
                sb.AppendLine($"                        {{ worldX = {point.WorldX}, worldY = {point.WorldY}, posX = {point.PosX}, posY = {point.PosY}, posZ = {point.PosZ} }},");
            }
            sb.AppendLine("                },");
        }
        sb.AppendLine("        }");
        sb.AppendLine("end");
        await File.WriteAllTextAsync(config.FilePath, sb.ToString());
    }

    private string DetectType(string value)
    {
        if (bool.TryParse(value, out _)) return "Boolean";
        if (int.TryParse(value, out _)) return "Integer";
        if (double.TryParse(value, out _)) return "Float";
        return "String";
    }

    public async Task<bool> AutoConfigureAsync()
    {
        try
        {
            var settings = _configService.GetAppSettings();
            var serverDir = settings.ServerDirectoryPath;
            var activeServer = settings.ActiveServer;

            if (string.IsNullOrEmpty(serverDir) || !Directory.Exists(serverDir))
            {
                _logger.LogWarning("[PzServer] AutoConfigure failed: ServerDirectoryPath is invalid.");
                return false;
            }

            bool changed = false;

            // 1. Zomboid Directory (usually the parent of the server dir or the server dir itself)
            // If empty, we assume serverDir is the Zomboid directory
            if (string.IsNullOrEmpty(settings.ZomboidDirectory))
            {
                settings.ZomboidDirectory = serverDir;
                changed = true;
            }

            // 2. INI File
            if (string.IsNullOrEmpty(settings.ServerIniPath))
            {
                var iniPath = Path.Combine(serverDir, $"{activeServer}.ini");
                if (File.Exists(iniPath))
                {
                    settings.ServerIniPath = iniPath;
                    changed = true;
                }
            }

            // 3. SandboxVars.lua
            if (string.IsNullOrEmpty(settings.SandboxVarsPath))
            {
                var luaPath = Path.Combine(serverDir, $"{activeServer}_SandboxVars.lua");
                if (File.Exists(luaPath))
                {
                    settings.SandboxVarsPath = luaPath;
                    changed = true;
                }
            }

            // 4. SpawnRegions.lua
            if (string.IsNullOrEmpty(settings.SpawnRegionsPath))
            {
                var spawnPath = Path.Combine(serverDir, $"{activeServer}_spawnregions.lua");
                if (File.Exists(spawnPath))
                {
                    settings.SpawnRegionsPath = spawnPath;
                    changed = true;
                }
            }

            if (changed)
            {
                await _configService.SaveAppSettingsAsync(settings);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PzServer] AutoConfigure threw an exception.");
            return false;
        }
    }
}

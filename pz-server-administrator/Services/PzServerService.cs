using System.Text;
using System.Text.RegularExpressions;
using pz_server_administrator.Models;

namespace pz_server_administrator.Services;

public class PzServerService : IPzServerService
{
    private readonly ILogger<PzServerService> _logger;

    public PzServerService(ILogger<PzServerService> logger)
    {
        _logger = logger;
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
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#") || trimmed.StartsWith(";")) continue;

            var parts = trimmed.Split('=', 2);
            if (parts.Length == 2)
            {
                config.Entries.Add(new PzConfigEntry
                {
                    Key = parts[0].Trim(),
                    Value = parts[1].Trim(),
                    Type = DetectType(parts[1].Trim())
                });
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
        // Simple regex for SandboxVars style: Key = Value,
        var regex = new Regex(@"(\w+)\s*=\s*([^,]+),", RegexOptions.Multiline);
        var matches = regex.Matches(content);

        foreach (Match match in matches)
        {
            config.Entries.Add(new PzConfigEntry
            {
                Key = match.Groups[1].Value,
                Value = match.Groups[2].Value.Trim().Trim('"').Trim('\''),
                Type = DetectType(match.Groups[2].Value.Trim())
            });
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

    private string DetectType(string value)
    {
        if (bool.TryParse(value, out _)) return "Boolean";
        if (int.TryParse(value, out _)) return "Integer";
        if (double.TryParse(value, out _)) return "Float";
        return "String";
    }
}

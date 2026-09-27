using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace pz_server_administrator.Services;

public interface IServerLoggerService
{
    List<LogFileInfo> GetAvailableLogFiles();
    Task<string> ReadLogFileAsync(string relativePath, int maxLines = 1000);
}

public class LogFileInfo
{
    public string Name { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime LastModified { get; set; }

    public string FormattedSize
    {
        get
        {
            if (Size < 1024) return $"{Size} B";
            if (Size < 1024 * 1024) return $"{Size / 1024.0:F2} KB";
            return $"{Size / (1024.0 * 1024.0):F2} MB";
        }
    }
}

public class ServerLoggerService : IServerLoggerService
{
    private readonly IConfigurationService _configurationService;

    public ServerLoggerService(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    private IEnumerable<string> GetPotentialLogRoots()
    {
        var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var config = _configurationService.GetConfiguration();

        var serverDir = config?.AppSettings?.ServerDirectoryPath;
        if (!string.IsNullOrEmpty(serverDir) && Directory.Exists(serverDir))
        {
            roots.Add(serverDir);
            var parent = Directory.GetParent(serverDir)?.FullName;
            if (!string.IsNullOrEmpty(parent) && Directory.Exists(parent))
            {
                roots.Add(parent);
            }
        }

        var zomboidDir = config?.AppSettings?.ZomboidDirectory;
        if (!string.IsNullOrEmpty(zomboidDir) && Directory.Exists(zomboidDir))
        {
            roots.Add(zomboidDir);
        }

        // Add application Serilog log roots (Docker persistent volume or local directory)
        if (Directory.Exists("/app/config/logs"))
        {
            roots.Add("/app/config");
        }
        var localLogsDir = Path.Combine(AppContext.BaseDirectory, "logs");
        if (Directory.Exists(localLogsDir))
        {
            roots.Add(AppContext.BaseDirectory);
        }

        return roots;
    }

    public List<LogFileInfo> GetAvailableLogFiles()
    {
        var roots = GetPotentialLogRoots();
        if (!roots.Any()) return new List<LogFileInfo>();

        var result = new List<LogFileInfo>();
        var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rootDir in roots)
        {
            // Add server-console.txt
            var serverConsole = Path.Combine(rootDir, "server-console.txt");
            if (File.Exists(serverConsole) && seenPaths.Add(serverConsole))
            {
                var info = new FileInfo(serverConsole);
                result.Add(new LogFileInfo
                {
                    Name = "server-console.txt",
                    RelativePath = "server-console.txt",
                    Size = info.Length,
                    LastModified = info.LastWriteTime
                });
            }

            // Add everything in Logs / logs (.txt and .log)
            var possibleLogDirs = new[] { Path.Combine(rootDir, "Logs"), Path.Combine(rootDir, "logs") };
            foreach (var logsDir in possibleLogDirs.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (Directory.Exists(logsDir))
                {
                    var files = Directory.GetFiles(logsDir, "*.*", SearchOption.AllDirectories)
                        .Where(f => f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".log", StringComparison.OrdinalIgnoreCase));

                    foreach (var file in files)
                    {
                        if (seenPaths.Add(file))
                        {
                            var info = new FileInfo(file);
                            result.Add(new LogFileInfo
                            {
                                Name = info.Name,
                                RelativePath = Path.GetRelativePath(rootDir, file),
                                Size = info.Length,
                                LastModified = info.LastWriteTime
                            });
                        }
                    }
                }
            }
        }

        return result.OrderByDescending(f => f.LastModified).ToList();
    }

    public async Task<string> ReadLogFileAsync(string relativePath, int maxLines = 1000)
    {
        // Path traversal protection
        if (string.IsNullOrWhiteSpace(relativePath) || relativePath.Contains("..")) return string.Empty;

        var roots = GetPotentialLogRoots();
        string? targetFile = null;

        foreach (var root in roots)
        {
            var candidate = Path.Combine(root, relativePath);
            if (File.Exists(candidate))
            {
                targetFile = candidate;
                break;
            }
        }

        if (targetFile == null) return string.Empty;
        var fullPath = targetFile;

        try
        {
            using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fileStream);

            // To avoid loading massive files fully into memory, we could either read all lines,
            // or just load them if less than reasonable size.
            // Using a simple reverse read is harder, but let's read all backwards if it's too big, or simply read everything.
            if (fileStream.Length > 10 * 1024 * 1024) // 10MB limit
            {
                return "File is too large to display entirely (>10MB). Consider downloading it via FTP.";
            }

            var allContent = await reader.ReadToEndAsync();
            var lines = allContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            var tailLines = lines.Skip(Math.Max(0, lines.Length - maxLines)).ToArray();
            return string.Join(Environment.NewLine, tailLines);
        }
        catch (Exception ex)
        {
            return $"Error al leer el archivo: {ex.Message}";
        }
    }
}

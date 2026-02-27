using pz_server_administrator.Models;

namespace pz_server_administrator.Services;

public interface IPzServerService
{
    Task<List<PzFile>> GetServerFilesAsync(string serverPath);
    Task<string> ReadFileContentAsync(string filePath);
    Task SaveFileContentAsync(string filePath, string content);
    Task<PzConfig> ParseConfigAsync(string filePath);
    Task SaveConfigAsync(PzConfig config);
}

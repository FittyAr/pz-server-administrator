namespace pz_server_administrator.Models;

public enum PzFileType
{
    Ini,
    Lua,
    Database,
    Other
}

public class PzFile
{
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public PzFileType Type { get; set; }
    public long SizeBytes { get; set; }
    public DateTime LastModified { get; set; }
}

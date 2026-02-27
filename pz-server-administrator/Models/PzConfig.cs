namespace pz_server_administrator.Models;

public class PzConfigEntry
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "String"; // String, Boolean, Integer, Float
    public List<string>? Options { get; set; }
}

public class PzConfig
{
    public string FilePath { get; set; } = string.Empty;
    public List<PzConfigEntry> Entries { get; set; } = new();
}

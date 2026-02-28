namespace pz_server_administrator.Models;

public class SpawnRegion
{
    public string Name { get; set; } = string.Empty;
    public string? File { get; set; }
    public string? ServerFile { get; set; }
}

public class SpawnPoint
{
    public int WorldX { get; set; }
    public int WorldY { get; set; }
    public int PosX { get; set; }
    public int PosY { get; set; }
    public int PosZ { get; set; }
}

public class SpawnPointsConfig
{
    public string FilePath { get; set; } = string.Empty;
    public Dictionary<string, List<SpawnPoint>> ProfessionPoints { get; set; } = new();
}

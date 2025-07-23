namespace pz_server_administrator.Models;

/// <summary>
/// Represents the main application settings configuration
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Path to the directory containing Project Zomboid server instances
    /// </summary>
    public string ServerDirectoryPath { get; set; } = string.Empty;

    /// <summary>
    /// Name of the currently active server
    /// </summary>
    public string ActiveServer { get; set; } = string.Empty;

    /// <summary>
    /// RCON connection configuration
    /// </summary>
    public RconSettings RCON { get; set; } = new();
}

/// <summary>
/// RCON connection settings
/// </summary>
public class RconSettings
{
    /// <summary>
    /// RCON server host address
    /// </summary>
    public string Host { get; set; } = "127.0.0.1";

    /// <summary>
    /// RCON server port
    /// </summary>
    public int Port { get; set; } = 27015;

    /// <summary>
    /// RCON authentication password
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
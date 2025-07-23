namespace pz_server_administrator.Models;

/// <summary>
/// Represents a user in the system
/// </summary>
public class User
{
    /// <summary>
    /// Username for authentication
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password for security
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// User role (Guest, Moderator, Administrator)
    /// </summary>
    public string Role { get; set; } = "Guest";
}

/// <summary>
/// Represents role permissions in the system
/// </summary>
public class RolePermissions
{
    /// <summary>
    /// Permission to edit server configuration files
    /// </summary>
    public bool AllowConfigEdit { get; set; }

    /// <summary>
    /// Permission to use RCON commands
    /// </summary>
    public bool AllowRcon { get; set; }

    /// <summary>
    /// Permission to manage server mods
    /// </summary>
    public bool AllowModManagement { get; set; }

    /// <summary>
    /// Permission to write to the database
    /// </summary>
    public bool AllowDatabaseWrite { get; set; }

    /// <summary>
    /// Permission to switch between servers
    /// </summary>
    public bool AllowServerSwitch { get; set; }
}

/// <summary>
/// Complete configuration structure from appsettings.zsm.json
/// </summary>
public class ZsmConfiguration
{
    /// <summary>
    /// Application settings
    /// </summary>
    public AppSettings AppSettings { get; set; } = new();

    /// <summary>
    /// List of system users
    /// </summary>
    public List<User> Users { get; set; } = new();

    /// <summary>
    /// Role permissions configuration
    /// </summary>
    public Dictionary<string, RolePermissions> Roles { get; set; } = new();
}
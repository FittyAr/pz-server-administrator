namespace pz_server_administrator.Models;

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
    public List<AuthUser> Users { get; set; } = new();

    /// <summary>
    /// Role permissions configuration
    /// </summary>
    public Dictionary<string, RolePermissions> Roles { get; set; } = new();
}
namespace pz_server_administrator.Models;

/// <summary>
/// Defines the permissions available for each user role
/// </summary>
public class RolePermissions
{
    /// <summary>
    /// Permission to edit server configuration files
    /// </summary>
    public bool AllowConfigEdit { get; set; }
    
    /// <summary>
    /// Permission to access RCON functionality
    /// </summary>
    public bool AllowRcon { get; set; }
    
    /// <summary>
    /// Permission to manage server mods
    /// </summary>
    public bool AllowModManagement { get; set; }
    
    /// <summary>
    /// Permission to write to the server database
    /// </summary>
    public bool AllowDatabaseWrite { get; set; }
    
    /// <summary>
    /// Permission to switch between different servers
    /// </summary>
    public bool AllowServerSwitch { get; set; }
    
    /// <summary>
    /// Permission to manage users and their roles
    /// </summary>
    public bool AllowUserManagement { get; set; }
    
    /// <summary>
    /// Gets all permissions as a dictionary for iteration
    /// </summary>
    /// <returns>Dictionary with permission names and their values</returns>
    public Dictionary<string, bool> GetPermissionsDictionary()
    {
        return new Dictionary<string, bool>
        {
            ["AllowConfigEdit"] = AllowConfigEdit,
            ["AllowRcon"] = AllowRcon,
            ["AllowModManagement"] = AllowModManagement,
            ["AllowDatabaseWrite"] = AllowDatabaseWrite,
            ["AllowServerSwitch"] = AllowServerSwitch,
            ["AllowUserManagement"] = AllowUserManagement
        };
    }
    
    /// <summary>
    /// Gets the default permissions for a specific role
    /// </summary>
    /// <param name="role">The user role</param>
    /// <returns>RolePermissions object with appropriate permissions set</returns>
    public static RolePermissions GetDefaultPermissions(UserRole role)
    {
        return role switch
        {
            UserRole.Guest => new RolePermissions
            {
                AllowConfigEdit = false,
                AllowRcon = false,
                AllowModManagement = false,
                AllowDatabaseWrite = false,
                AllowServerSwitch = false,
                AllowUserManagement = false
            },
            UserRole.Moderator => new RolePermissions
            {
                AllowConfigEdit = false,
                AllowRcon = true,
                AllowModManagement = false,
                AllowDatabaseWrite = false,
                AllowServerSwitch = false,
                AllowUserManagement = false
            },
            UserRole.Administrator => new RolePermissions
            {
                AllowConfigEdit = true,
                AllowRcon = true,
                AllowModManagement = true,
                AllowDatabaseWrite = true,
                AllowServerSwitch = true,
                AllowUserManagement = true
            },
            _ => new RolePermissions()
        };
    }
}
namespace pz_server_administrator.Models;

/// <summary>
/// Represents an authenticated user in the system
/// </summary>
public class AuthUser
{
    /// <summary>
    /// Unique username for the user
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Hashed password for security
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>
    /// User's role in the system
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Guest;
    
    /// <summary>
    /// Indicates if the user is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Date when the user was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Date when the user last logged in
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// Gets the permissions for this user based on their role
    /// </summary>
    /// <returns>RolePermissions object</returns>
    public RolePermissions GetPermissions()
    {
        return RolePermissions.GetDefaultPermissions(Role);
    }
    
    /// <summary>
    /// Checks if the user has a specific permission
    /// </summary>
    /// <param name="permissionCheck">Function to check specific permission</param>
    /// <returns>True if user has the permission</returns>
    public bool HasPermission(Func<RolePermissions, bool> permissionCheck)
    {
        var permissions = GetPermissions();
        return permissionCheck(permissions);
    }
}
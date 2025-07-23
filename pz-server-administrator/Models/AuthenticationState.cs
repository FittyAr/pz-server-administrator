namespace pz_server_administrator.Models;

/// <summary>
/// Represents the current authentication state of the application
/// </summary>
public class AuthenticationState
{
    /// <summary>
    /// Indicates if a user is currently authenticated
    /// </summary>
    public bool IsAuthenticated { get; set; } = false;
    
    /// <summary>
    /// The currently authenticated user, null if not authenticated
    /// </summary>
    public AuthUser? CurrentUser { get; set; }
    
    /// <summary>
    /// The current user's role, defaults to Guest
    /// </summary>
    public UserRole CurrentRole => CurrentUser?.Role ?? UserRole.Guest;
    
    /// <summary>
    /// The current user's permissions
    /// </summary>
    public RolePermissions CurrentPermissions => CurrentUser?.GetPermissions() ?? RolePermissions.GetDefaultPermissions(UserRole.Guest);
    
    /// <summary>
    /// Time when the current session started
    /// </summary>
    public DateTime? SessionStartTime { get; set; }
    
    /// <summary>
    /// Checks if the current user has a specific permission
    /// </summary>
    /// <param name="permissionCheck">Function to check specific permission</param>
    /// <returns>True if user has the permission</returns>
    public bool HasPermission(Func<RolePermissions, bool> permissionCheck)
    {
        return permissionCheck(CurrentPermissions);
    }
    
    /// <summary>
    /// Resets the authentication state to unauthenticated
    /// </summary>
    public void Reset()
    {
        IsAuthenticated = false;
        CurrentUser = null;
        SessionStartTime = null;
    }
    
    /// <summary>
    /// Sets the authentication state for a successful login
    /// </summary>
    /// <param name="user">The authenticated user</param>
    public void SetAuthenticated(AuthUser user)
    {
        IsAuthenticated = true;
        CurrentUser = user;
        SessionStartTime = DateTime.UtcNow;
        
        // Update last login time
        if (CurrentUser != null)
        {
            CurrentUser.LastLoginAt = DateTime.UtcNow;
        }
    }
}
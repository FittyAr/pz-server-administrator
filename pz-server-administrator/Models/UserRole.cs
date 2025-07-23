namespace pz_server_administrator.Models;

/// <summary>
/// Enumeration defining the different user roles in the application
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Guest user with read-only access, no authentication required
    /// </summary>
    Guest = 0,
    
    /// <summary>
    /// Moderator with limited administrative functions like RCON access
    /// </summary>
    Moderator = 1,
    
    /// <summary>
    /// Administrator with full access to all application features
    /// </summary>
    Administrator = 2
}
using pz_server_administrator.Models;

namespace pz_server_administrator.Services;

/// <summary>
/// Interface for authentication operations
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Event triggered when authentication state changes
    /// </summary>
    event Action<AuthenticationState>? AuthenticationStateChanged;
    
    /// <summary>
    /// Gets the current authentication state
    /// </summary>
    AuthenticationState CurrentState { get; }
    
    /// <summary>
    /// Attempts to authenticate a user with username and password
    /// </summary>
    /// <param name="username">The username</param>
    /// <param name="password">The plain text password</param>
    /// <returns>True if authentication was successful</returns>
    Task<bool> LoginAsync(string username, string password);
    
    /// <summary>
    /// Logs out the current user
    /// </summary>
    Task LogoutAsync();
    
    /// <summary>
    /// Checks if the current user has a specific permission
    /// </summary>
    /// <param name="permissionCheck">Function to check specific permission</param>
    /// <returns>True if user has the permission</returns>
    bool HasPermission(Func<RolePermissions, bool> permissionCheck);
    
    /// <summary>
    /// Gets all users from the configuration
    /// </summary>
    /// <returns>List of all users</returns>
    Task<List<AuthUser>> GetAllUsersAsync();
    
    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="username">The username</param>
    /// <param name="password">The plain text password</param>
    /// <param name="role">The user role</param>
    /// <returns>True if user was created successfully</returns>
    Task<bool> CreateUserAsync(string username, string password, UserRole role);
    
    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="username">The username to update</param>
    /// <param name="newPassword">The new password (optional)</param>
    /// <param name="newRole">The new role (optional)</param>
    /// <param name="isActive">The active status (optional)</param>
    /// <returns>True if user was updated successfully</returns>
    Task<bool> UpdateUserAsync(string username, string? newPassword = null, UserRole? newRole = null, bool? isActive = null);
    
    /// <summary>
    /// Deletes a user
    /// </summary>
    /// <param name="username">The username to delete</param>
    /// <returns>True if user was deleted successfully</returns>
    Task<bool> DeleteUserAsync(string username);
    
    /// <summary>
    /// Checks if a username already exists
    /// </summary>
    /// <param name="username">The username to check</param>
    /// <returns>True if username exists</returns>
    Task<bool> UserExistsAsync(string username);
}
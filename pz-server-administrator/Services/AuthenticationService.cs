using Microsoft.Extensions.Logging;
using pz_server_administrator.Models;
using System.Text.Json;

namespace pz_server_administrator.Services;

/// <summary>
/// Implementation of authentication service
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IConfigurationService _configurationService;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly ILogger<AuthenticationService> _logger;
    private AuthenticationState _currentState;

    /// <summary>
    /// Event triggered when authentication state changes
    /// </summary>
    public event Action<AuthenticationState>? AuthenticationStateChanged;

    /// <summary>
    /// Gets the current authentication state
    /// </summary>
    public AuthenticationState CurrentState => _currentState;

    /// <summary>
    /// Initializes a new instance of the AuthenticationService
    /// </summary>
    /// <param name="configurationService">Configuration service for user data</param>
    /// <param name="passwordHashingService">Password hashing service</param>
    /// <param name="logger">Logger instance</param>
    public AuthenticationService(
        IConfigurationService configurationService,
        IPasswordHashingService passwordHashingService,
        ILogger<AuthenticationService> logger)
    {
        _configurationService = configurationService;
        _passwordHashingService = passwordHashingService;
        _logger = logger;
        _currentState = new AuthenticationState();
    }

    /// <summary>
    /// Attempts to authenticate a user with username and password
    /// </summary>
    /// <param name="username">The username</param>
    /// <param name="password">The plain text password</param>
    /// <returns>True if authentication was successful</returns>
    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Login attempt with empty username or password");
                return false;
            }

            var users = await GetAllUsersAsync();
            var user = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                _logger.LogWarning("Login attempt with non-existent username: {Username}", username);
                return false;
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt with inactive user: {Username}", username);
                return false;
            }

            if (!_passwordHashingService.VerifyPassword(password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for user: {Username}", username);
                return false;
            }

            // Successful authentication
            _currentState.SetAuthenticated(user);
            _logger.LogInformation("User {Username} logged in successfully with role {Role}", username, user.Role);

            // Update last login time in configuration
            await UpdateUserAsync(username, isActive: true);

            // Notify state change
            AuthenticationStateChanged?.Invoke(_currentState);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login attempt for user: {Username}", username);
            return false;
        }
    }

    /// <summary>
    /// Logs out the current user
    /// </summary>
    public async Task LogoutAsync()
    {
        try
        {
            var currentUser = _currentState.CurrentUser?.Username;
            _currentState.Reset();

            _logger.LogInformation("User {Username} logged out", currentUser ?? "Unknown");

            // Notify state change
            AuthenticationStateChanged?.Invoke(_currentState);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
        }
    }

    /// <summary>
    /// Checks if the current user has a specific permission
    /// </summary>
    /// <param name="permissionCheck">Function to check specific permission</param>
    /// <returns>True if user has the permission</returns>
    public bool HasPermission(Func<RolePermissions, bool> permissionCheck)
    {
        return _currentState.HasPermission(permissionCheck);
    }

    /// <summary>
    /// Gets all users from the configuration
    /// </summary>
    /// <returns>List of all users</returns>
    public Task<List<AuthUser>> GetAllUsersAsync()
    {
        try
        {
            var config = _configurationService.GetConfiguration();
            return Task.FromResult(config.Users ?? new List<AuthUser>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return Task.FromResult(new List<AuthUser>());
        }
    }

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="username">The username</param>
    /// <param name="password">The plain text password</param>
    /// <param name="role">The user role</param>
    /// <returns>True if user was created successfully</returns>
    public async Task<bool> CreateUserAsync(string username, string password, UserRole role)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Attempt to create user with empty username or password");
                return false;
            }

            if (await UserExistsAsync(username))
            {
                _logger.LogWarning("Attempt to create user with existing username: {Username}", username);
                return false;
            }

            var config = _configurationService.GetConfiguration();
            config.Users ??= new List<AuthUser>();

            var newUser = new AuthUser
            {
                Username = username,
                PasswordHash = _passwordHashingService.HashPassword(password),
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            config.Users.Add(newUser);
            _configurationService.SaveConfiguration(config);

            _logger.LogInformation("User {Username} created with role {Role}", username, role);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Username}", username);
            return false;
        }
    }

    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="username">The username to update</param>
    /// <param name="newPassword">The new password (optional)</param>
    /// <param name="newRole">The new role (optional)</param>
    /// <param name="isActive">The active status (optional)</param>
    /// <returns>True if user was updated successfully</returns>
    public Task<bool> UpdateUserAsync(string username, string? newPassword = null, UserRole? newRole = null, bool? isActive = null)
    {
        try
        {
            var config = _configurationService.GetConfiguration();
            var user = config.Users?.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                _logger.LogWarning("Attempt to update non-existent user: {Username}", username);
                return Task.FromResult(false);
            }

            bool hasChanges = false;

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                user.PasswordHash = _passwordHashingService.HashPassword(newPassword);
                hasChanges = true;
            }

            if (newRole.HasValue && newRole.Value != user.Role)
            {
                user.Role = newRole.Value;
                hasChanges = true;
            }

            if (isActive.HasValue && isActive.Value != user.IsActive)
            {
                user.IsActive = isActive.Value;
                hasChanges = true;
            }

            if (hasChanges)
            {
                _configurationService.SaveConfiguration(config);
                _logger.LogInformation("User {Username} updated", username);
            }

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {Username}", username);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Deletes a user
    /// </summary>
    /// <param name="username">The username to delete</param>
    /// <returns>True if user was deleted successfully</returns>
    public Task<bool> DeleteUserAsync(string username)
    {
        try
        {
            var config = _configurationService.GetConfiguration();
            var userIndex = config.Users?.FindIndex(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)) ?? -1;

            if (userIndex == -1)
            {
                _logger.LogWarning("Attempt to delete non-existent user: {Username}", username);
                return Task.FromResult(false);
            }

            // Prevent deleting the last administrator
            var adminCount = config.Users?.Count(u => u.Role == UserRole.Administrator && u.IsActive) ?? 0;
            var userToDelete = config.Users![userIndex];

            if (userToDelete.Role == UserRole.Administrator && adminCount <= 1)
            {
                _logger.LogWarning("Attempt to delete the last active administrator: {Username}", username);
                return Task.FromResult(false);
            }

            config.Users!.RemoveAt(userIndex);
            _configurationService.SaveConfiguration(config);

            _logger.LogInformation("User {Username} deleted", username);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {Username}", username);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Checks if a username already exists
    /// </summary>
    /// <param name="username">The username to check</param>
    /// <returns>True if username exists</returns>
    public async Task<bool> UserExistsAsync(string username)
    {
        try
        {
            var users = await GetAllUsersAsync();
            return users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user exists: {Username}", username);
            return false;
        }
    }
}
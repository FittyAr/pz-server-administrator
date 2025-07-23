namespace pz_server_administrator.Services;

/// <summary>
/// Interface for password hashing operations
/// </summary>
public interface IPasswordHashingService
{
    /// <summary>
    /// Hashes a plain text password using a secure algorithm
    /// </summary>
    /// <param name="password">The plain text password to hash</param>
    /// <returns>The hashed password</returns>
    string HashPassword(string password);
    
    /// <summary>
    /// Verifies a plain text password against a hashed password
    /// </summary>
    /// <param name="password">The plain text password to verify</param>
    /// <param name="hashedPassword">The hashed password to compare against</param>
    /// <returns>True if the password matches the hash</returns>
    bool VerifyPassword(string password, string hashedPassword);
}
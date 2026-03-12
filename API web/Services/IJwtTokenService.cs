using API_web.Models;

namespace API_web.Services;

/// <summary>
/// Interface for JWT token operations
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT token for the specified user
    /// </summary>
    /// <param name="user">The user to generate a token for</param>
    /// <returns>The generated JWT token string</returns>
    string GenerateToken(User user);

    /// <summary>
    /// Validates a JWT token and extracts the user ID
    /// </summary>
    /// <param name="token">The JWT token to validate</param>
    /// <returns>The user ID if valid, null otherwise</returns>
    long? ValidateToken(string token);
}

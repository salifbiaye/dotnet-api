namespace API_web.Models;

/// <summary>
/// Response model for user login
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// The JWT token for authenticating subsequent requests
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    public string Token { get; set; } = null!;

    /// <summary>
    /// The username of the authenticated user
    /// </summary>
    /// <example>john_doe</example>
    public string Username { get; set; } = null!;

    /// <summary>
    /// The role of the authenticated user
    /// </summary>
    /// <example>user</example>
    public string Role { get; set; } = null!;
}

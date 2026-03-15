namespace API_web.Models;

/// <summary>
/// Response model for user registration
/// </summary>
public class RegisterResponse
{
    /// <summary>
    /// The unique identifier for the created user
    /// </summary>
    /// <example>507f1f77bcf86cd799439011</example>
    public string Id { get; set; } = null!;

    /// <summary>
    /// The username of the created user
    /// </summary>
    /// <example>john_doe</example>
    public string Username { get; set; } = null!;

    /// <summary>
    /// The role assigned to the user
    /// </summary>
    /// <example>user</example>
    public string Role { get; set; } = null!;
}

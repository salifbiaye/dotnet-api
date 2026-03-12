using System.ComponentModel.DataAnnotations;

namespace API_web.Models;

/// <summary>
/// Request model for user login
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// The username for authentication
    /// </summary>
    /// <example>john_doe</example>
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = null!;

    /// <summary>
    /// The password for authentication
    /// </summary>
    /// <example>SecurePassword123!</example>
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = null!;
}

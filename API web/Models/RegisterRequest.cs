using System.ComponentModel.DataAnnotations;

namespace API_web.Models;

/// <summary>
/// Request model for user registration
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// The username for the new account (must be unique)
    /// </summary>
    /// <example>john_doe</example>
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public string Username { get; set; } = null!;

    /// <summary>
    /// The password for the new account
    /// </summary>
    /// <example>SecurePassword123!</example>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = null!;

    /// <summary>
    /// The role to assign to the user (default: "user"). Valid values: "user", "admin"
    /// </summary>
    /// <example>user</example>
    public string Role { get; set; } = "user";
}

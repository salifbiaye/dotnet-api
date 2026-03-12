using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_web.Models;
using API_web.Services;

namespace API_web.Controllers;

/// <summary>
/// Handles user authentication and registration
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly TodoContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(TodoContext context, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService, ILogger<AuthController> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">Registration details including username, password, and role</param>
    /// <returns>The created user information</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/register
    ///     {
    ///        "username": "john_doe",
    ///        "password": "SecurePassword123!",
    ///        "role": "user"
    ///     }
    /// 
    /// Valid roles: "user" (default), "admin"
    /// 
    /// **Authentication Required:** No
    /// </remarks>
    /// <response code="201">User successfully registered</response>
    /// <response code="400">Invalid request or username already exists</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username and password are required" });
        }

        // Check if username already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (existingUser != null)
        {
            return BadRequest(new { message = "Username already exists" });
        }

        // Hash the password
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // Create new user
        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash,
            Role = string.IsNullOrWhiteSpace(request.Role) ? "user" : request.Role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Return response
        var response = new RegisterResponse
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role
        };

        return CreatedAtAction(nameof(Register), new { id = user.Id }, response);
    }

    /// <summary>
    /// Authenticate a user and return a JWT token
    /// </summary>
    /// <param name="request">Login credentials including username and password</param>
    /// <returns>JWT token and user information</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/auth/login
    ///     {
    ///        "username": "john_doe",
    ///        "password": "SecurePassword123!"
    ///     }
    /// 
    /// The returned JWT token should be included in the Authorization header for protected endpoints:
    /// 
    ///     Authorization: Bearer {token}
    /// 
    /// **Authentication Required:** No
    /// </remarks>
    /// <response code="200">Login successful, returns JWT token</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Login failed for username '{Username}': Missing credentials", request.Username ?? "(empty)");
            return Unauthorized(new { message = "Username and password are required" });
        }

        // Find user by username
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
        {
            _logger.LogWarning("Login failed for username '{Username}': User not found", request.Username);
            return Unauthorized(new { message = "Invalid username or password" });
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed for username '{Username}': Invalid password", request.Username);
            return Unauthorized(new { message = "Invalid username or password" });
        }

        // Generate JWT token
        var token = _jwtTokenService.GenerateToken(user);

        // Log successful authentication
        _logger.LogInformation("User '{Username}' successfully authenticated at {Timestamp}", user.Username, DateTime.UtcNow);

        // Return response
        var response = new LoginResponse
        {
            Token = token,
            Username = user.Username,
            Role = user.Role
        };

        return Ok(response);
    }
}


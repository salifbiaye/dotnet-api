using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using API_web.Controllers;
using API_web.Models;
using API_web.Services;
using Xunit;
using Moq;

namespace API.Tests;

public class AuthControllerTests
{
    private TodoContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TodoContext(options);
    }

    private IJwtTokenService CreateJwtTokenService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JWT_SECRET_KEY"] = "test-secret-key-that-is-at-least-32-characters-long",
                ["JWT_EXPIRATION_MINUTES"] = "60"
            })
            .Build();

        var logger = new LoggerFactory().CreateLogger<JwtTokenService>();
        return new JwtTokenService(configuration, logger);
    }

    private ILogger<AuthController> CreateLogger()
    {
        return new Mock<ILogger<AuthController>>().Object;
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var context = CreateContext();
        var passwordHasher = new PasswordHasher();
        var jwtTokenService = CreateJwtTokenService();
        var logger = CreateLogger();
        var controller = new AuthController(context, passwordHasher, jwtTokenService, logger);
        var request = new RegisterRequest
        {
            Username = "testuser",
            Password = "password123",
            Role = "user"
        };

        // Act
        var result = await controller.Register(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
        var response = Assert.IsType<RegisterResponse>(createdResult.Value);
        Assert.Equal("testuser", response.Username);
        Assert.Equal("user", response.Role);
    }

    [Fact]
    public async Task Register_WithValidData_StoresHashedPassword()
    {
        // Arrange
        var context = CreateContext();
        var passwordHasher = new PasswordHasher();
        var jwtTokenService = CreateJwtTokenService();
        var logger = CreateLogger();
        var controller = new AuthController(context, passwordHasher, jwtTokenService, logger);
        var request = new RegisterRequest
        {
            Username = "testuser",
            Password = "password123",
            Role = "user"
        };

        // Act
        await controller.Register(request);

        // Assert
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
        Assert.NotNull(user);
        Assert.NotEqual("password123", user.PasswordHash);
        Assert.True(passwordHasher.VerifyPassword("password123", user.PasswordHash));
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsBadRequest()
    {
        // Arrange
        var context = CreateContext();
        var passwordHasher = new PasswordHasher();
        var jwtTokenService = CreateJwtTokenService();
        var logger = CreateLogger();
        var controller = new AuthController(context, passwordHasher, jwtTokenService, logger);
        
        // Create existing user
        context.Users.Add(new User
        {
            Username = "existinguser",
            PasswordHash = passwordHasher.HashPassword("password123"),
            Role = "user"
        });
        await context.SaveChangesAsync();

        var request = new RegisterRequest
        {
            Username = "existinguser",
            Password = "newpassword",
            Role = "user"
        };

        // Act
        var result = await controller.Register(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Register_WithEmptyUsername_ReturnsBadRequest()
    {
        // Arrange
        var context = CreateContext();
        var passwordHasher = new PasswordHasher();
        var jwtTokenService = CreateJwtTokenService();
        var logger = CreateLogger();
        var controller = new AuthController(context, passwordHasher, jwtTokenService, logger);
        var request = new RegisterRequest
        {
            Username = "",
            Password = "password123",
            Role = "user"
        };

        // Act
        var result = await controller.Register(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Register_WithEmptyPassword_ReturnsBadRequest()
    {
        // Arrange
        var context = CreateContext();
        var passwordHasher = new PasswordHasher();
        var jwtTokenService = CreateJwtTokenService();
        var logger = CreateLogger();
        var controller = new AuthController(context, passwordHasher, jwtTokenService, logger);
        var request = new RegisterRequest
        {
            Username = "testuser",
            Password = "",
            Role = "user"
        };

        // Act
        var result = await controller.Register(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task Register_WithDefaultRole_CreatesUserWithUserRole()
    {
        // Arrange
        var context = CreateContext();
        var passwordHasher = new PasswordHasher();
        var jwtTokenService = CreateJwtTokenService();
        var logger = CreateLogger();
        var controller = new AuthController(context, passwordHasher, jwtTokenService, logger);
        var request = new RegisterRequest
        {
            Username = "testuser",
            Password = "password123",
            Role = ""
        };

        // Act
        var result = await controller.Register(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<RegisterResponse>(createdResult.Value);
        Assert.Equal("user", response.Role);
    }

    [Fact]
    public async Task Register_WithAdminRole_CreatesAdminUser()
    {
        // Arrange
        var context = CreateContext();
        var passwordHasher = new PasswordHasher();
        var jwtTokenService = CreateJwtTokenService();
        var logger = CreateLogger();
        var controller = new AuthController(context, passwordHasher, jwtTokenService, logger);
        var request = new RegisterRequest
        {
            Username = "adminuser",
            Password = "password123",
            Role = "admin"
        };

        // Act
        var result = await controller.Register(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<RegisterResponse>(createdResult.Value);
        Assert.Equal("admin", response.Role);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var context = CreateContext();
        var passwordHasher = new PasswordHasher();
        var jwtTokenService = CreateJwtTokenService();
        var logger = CreateLogger();
        var controller = new AuthController(context, passwordHasher, jwtTokenService, logger);
        
        // Create a user
        var user = new User
        {
            Username = "testuser",
            PasswordHash = passwordHasher.HashPassword("password123"),
            Role = "user"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "password123"
        };

        // Act
        var result = await controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        var response = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.NotNull(response.Token);
        Assert.NotEmpty(response.Token);
        Assert.Equal("testuser", response.Username);
        Assert.Equal("user", response.Role);
    }

    [Fact]
    public async Task Login_WithInvalidUsername_ReturnsUnauthorized()
    {
        // Arrange
        var context = CreateContext();
        var passwordHasher = new PasswordHasher();
        var jwtTokenService = CreateJwtTokenService();
        var logger = CreateLogger();
        var controller = new AuthController(context, passwordHasher, jwtTokenService, logger);

        var request = new LoginRequest
        {
            Username = "nonexistentuser",
            Password = "password123"
        };

        // Act
        var result = await controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var context = CreateContext();
        var passwordHasher = new PasswordHasher();
        var jwtTokenService = CreateJwtTokenService();
        var logger = CreateLogger();
        var controller = new AuthController(context, passwordHasher, jwtTokenService, logger);
        
        // Create a user
        var user = new User
        {
            Username = "testuser",
            PasswordHash = passwordHasher.HashPassword("password123"),
            Role = "user"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        // Act
        var result = await controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task Login_WithEmptyUsername_ReturnsUnauthorized()
    {
        // Arrange
        var context = CreateContext();
        var passwordHasher = new PasswordHasher();
        var jwtTokenService = CreateJwtTokenService();
        var logger = CreateLogger();
        var controller = new AuthController(context, passwordHasher, jwtTokenService, logger);

        var request = new LoginRequest
        {
            Username = "",
            Password = "password123"
        };

        // Act
        var result = await controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ReturnsUnauthorized()
    {
        // Arrange
        var context = CreateContext();
        var passwordHasher = new PasswordHasher();
        var jwtTokenService = CreateJwtTokenService();
        var logger = CreateLogger();
        var controller = new AuthController(context, passwordHasher, jwtTokenService, logger);

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = ""
        };

        // Act
        var result = await controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task Login_WithAdminUser_ReturnsTokenWithAdminRole()
    {
        // Arrange
        var context = CreateContext();
        var passwordHasher = new PasswordHasher();
        var jwtTokenService = CreateJwtTokenService();
        var logger = CreateLogger();
        var controller = new AuthController(context, passwordHasher, jwtTokenService, logger);
        
        // Create an admin user
        var user = new User
        {
            Username = "adminuser",
            PasswordHash = passwordHasher.HashPassword("adminpass"),
            Role = "admin"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Username = "adminuser",
            Password = "adminpass"
        };

        // Act
        var result = await controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.Equal("admin", response.Role);
        Assert.NotNull(response.Token);
    }
}

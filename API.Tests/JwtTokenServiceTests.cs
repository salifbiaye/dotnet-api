using API_web.Models;
using API_web.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace API.Tests;

public class JwtTokenServiceTests
{
    private readonly Mock<ILogger<JwtTokenService>> _mockLogger;
    private readonly IConfiguration _configuration;

    public JwtTokenServiceTests()
    {
        _mockLogger = new Mock<ILogger<JwtTokenService>>();
        
        // Setup configuration with test values
        var configData = new Dictionary<string, string?>
        {
            { "JWT_SECRET_KEY", "test-secret-key-that-is-long-enough-for-hmac-sha256-algorithm" },
            { "JWT_EXPIRATION_MINUTES", "60" }
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
    }

    [Fact]
    public void GenerateToken_WithValidUser_ReturnsToken()
    {
        // Arrange
        var service = new JwtTokenService(_configuration, _mockLogger.Object);
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Role = "user",
            PasswordHash = "hash"
        };

        // Act
        var token = service.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateToken_WithoutSecretKey_ThrowsException()
    {
        // Arrange
        var emptyConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        var service = new JwtTokenService(emptyConfig, _mockLogger.Object);
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Role = "user",
            PasswordHash = "hash"
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => service.GenerateToken(user));
    }

    [Fact]
    public void GenerateToken_IncludesUserClaims()
    {
        // Arrange
        var service = new JwtTokenService(_configuration, _mockLogger.Object);
        var user = new User
        {
            Id = 123,
            Username = "testuser",
            Role = "admin",
            PasswordHash = "hash"
        };

        // Act
        var token = service.GenerateToken(user);
        var userId = service.ValidateToken(token);

        // Assert
        Assert.NotNull(userId);
        Assert.Equal(123, userId.Value);
    }

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsUserId()
    {
        // Arrange
        var service = new JwtTokenService(_configuration, _mockLogger.Object);
        var user = new User
        {
            Id = 42,
            Username = "testuser",
            Role = "user",
            PasswordHash = "hash"
        };
        var token = service.GenerateToken(user);

        // Act
        var userId = service.ValidateToken(token);

        // Assert
        Assert.NotNull(userId);
        Assert.Equal(42, userId.Value);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var service = new JwtTokenService(_configuration, _mockLogger.Object);
        var invalidToken = "invalid.token.here";

        // Act
        var userId = service.ValidateToken(invalidToken);

        // Assert
        Assert.Null(userId);
    }

    [Fact]
    public void ValidateToken_WithMalformedToken_ReturnsNull()
    {
        // Arrange
        var service = new JwtTokenService(_configuration, _mockLogger.Object);
        var malformedToken = "not-a-jwt-token";

        // Act
        var userId = service.ValidateToken(malformedToken);

        // Assert
        Assert.Null(userId);
    }

    [Fact]
    public void ValidateToken_WithExpiredToken_ReturnsNull()
    {
        // Arrange
        var expiredConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "JWT_SECRET_KEY", "test-secret-key-that-is-long-enough-for-hmac-sha256-algorithm" },
                { "JWT_EXPIRATION_MINUTES", "0" } // Expire immediately
            })
            .Build();
        var service = new JwtTokenService(expiredConfig, _mockLogger.Object);
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Role = "user",
            PasswordHash = "hash"
        };
        var token = service.GenerateToken(user);

        // Wait a moment to ensure token expires
        Thread.Sleep(100);

        // Act
        var userId = service.ValidateToken(token);

        // Assert
        Assert.Null(userId);
    }

    [Fact]
    public void ValidateToken_WithoutSecretKey_ReturnsNull()
    {
        // Arrange
        var emptyConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        var service = new JwtTokenService(emptyConfig, _mockLogger.Object);
        var token = "some.token.here";

        // Act
        var userId = service.ValidateToken(token);

        // Assert
        Assert.Null(userId);
    }

    [Fact]
    public void GenerateToken_UsesConfiguredExpirationMinutes()
    {
        // Arrange
        var customConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "JWT_SECRET_KEY", "test-secret-key-that-is-long-enough-for-hmac-sha256-algorithm" },
                { "JWT_EXPIRATION_MINUTES", "120" }
            })
            .Build();
        var service = new JwtTokenService(customConfig, _mockLogger.Object);
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Role = "user",
            PasswordHash = "hash"
        };

        // Act
        var token = service.GenerateToken(user);
        var userId = service.ValidateToken(token);

        // Assert - Token should be valid since it has 120 minutes expiration
        Assert.NotNull(userId);
        Assert.Equal(1, userId.Value);
    }

    [Fact]
    public void GenerateToken_DefaultsTo60MinutesWhenNotConfigured()
    {
        // Arrange
        var configWithoutExpiration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "JWT_SECRET_KEY", "test-secret-key-that-is-long-enough-for-hmac-sha256-algorithm" }
            })
            .Build();
        var service = new JwtTokenService(configWithoutExpiration, _mockLogger.Object);
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Role = "user",
            PasswordHash = "hash"
        };

        // Act
        var token = service.GenerateToken(user);
        var userId = service.ValidateToken(token);

        // Assert - Token should be valid with default 60 minutes
        Assert.NotNull(userId);
        Assert.Equal(1, userId.Value);
    }
}

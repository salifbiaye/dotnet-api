using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using API_web.Middleware;
using System.Security.Claims;

namespace API.Tests;

public class AuthorizationLoggingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_Logs401WithAuthorizationHeader_LogsJwtValidationFailure()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AuthorizationLoggingMiddleware>>();
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/todoitems";
        context.Request.Headers["Authorization"] = "Bearer invalid-token";
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

        RequestDelegate next = (HttpContext ctx) =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };

        var middleware = new AuthorizationLoggingMiddleware(next, loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("JWT token validation failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_Logs401WithoutAuthorizationHeader_LogsUnauthorizedAccess()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AuthorizationLoggingMiddleware>>();
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/todoitems";
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

        RequestDelegate next = (HttpContext ctx) =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };

        var middleware = new AuthorizationLoggingMiddleware(next, loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unauthorized access attempt")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_Logs403Forbidden_LogsAuthorizationFailureWithUserAndRole()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AuthorizationLoggingMiddleware>>();
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/todoitems";
        
        // Set up authenticated user with role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, "user")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        context.User = new ClaimsPrincipal(identity);

        RequestDelegate next = (HttpContext ctx) =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };

        var middleware = new AuthorizationLoggingMiddleware(next, loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("Authorization failed") &&
                    v.ToString()!.Contains("testuser") &&
                    v.ToString()!.Contains("user") &&
                    v.ToString()!.Contains("POST /api/todoitems")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_Returns200_DoesNotLog()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<AuthorizationLoggingMiddleware>>();
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/todoitems";
        context.Response.StatusCode = StatusCodes.Status200OK;

        RequestDelegate next = (HttpContext ctx) =>
        {
            ctx.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        };

        var middleware = new AuthorizationLoggingMiddleware(next, loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}

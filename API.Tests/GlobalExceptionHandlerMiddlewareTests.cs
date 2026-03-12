using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Moq;
using API_web.Middleware;
using System.Text.Json;

namespace API.Tests;

public class GlobalExceptionHandlerMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenNoException_CallsNextMiddleware()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        var mockEnvironment = new Mock<IHostEnvironment>();
        var context = new DefaultHttpContext();
        var nextCalled = false;
        
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, mockLogger.Object, mockEnvironment.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_LogsErrorWithStackTrace()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        var mockEnvironment = new Mock<IHostEnvironment>();
        mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");
        
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var testException = new InvalidOperationException("Test exception");
        
        RequestDelegate next = (HttpContext ctx) =>
        {
            throw testException;
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, mockLogger.Object, mockEnvironment.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unhandled exception occurred")),
                testException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_Returns500StatusCode()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        var mockEnvironment = new Mock<IHostEnvironment>();
        mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");
        
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        RequestDelegate next = (HttpContext ctx) =>
        {
            throw new InvalidOperationException("Test exception");
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, mockLogger.Object, mockEnvironment.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(500, context.Response.StatusCode);
        Assert.StartsWith("application/json", context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_InDevelopment_ReturnsDetailedErrorMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        var mockEnvironment = new Mock<IHostEnvironment>();
        mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
        
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        var exceptionMessage = "Detailed test exception";
        RequestDelegate next = (HttpContext ctx) =>
        {
            throw new InvalidOperationException(exceptionMessage);
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, mockLogger.Object, mockEnvironment.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        Assert.Equal(exceptionMessage, response.GetProperty("message").GetString());
        Assert.Equal("InvalidOperationException", response.GetProperty("type").GetString());
    }

    [Fact]
    public async Task InvokeAsync_InProduction_ReturnsGenericErrorMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        var mockEnvironment = new Mock<IHostEnvironment>();
        mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");
        
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        
        RequestDelegate next = (HttpContext ctx) =>
        {
            throw new InvalidOperationException("Detailed test exception");
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, mockLogger.Object, mockEnvironment.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        Assert.DoesNotContain("Detailed test exception", response.GetProperty("message").GetString());
        Assert.Contains("contact support", response.GetProperty("message").GetString());
    }

    [Fact]
    public async Task InvokeAsync_LogsRequestMethodAndPath()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        var mockEnvironment = new Mock<IHostEnvironment>();
        mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");
        
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Path = "/api/test";
        context.Response.Body = new MemoryStream();
        
        var testException = new InvalidOperationException("Test exception");
        
        RequestDelegate next = (HttpContext ctx) =>
        {
            throw testException;
        };

        var middleware = new GlobalExceptionHandlerMiddleware(next, mockLogger.Object, mockEnvironment.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("POST") && v.ToString()!.Contains("/api/test")),
                testException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

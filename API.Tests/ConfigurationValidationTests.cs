using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace API.Tests;

public class ConfigurationValidationTests
{
    [Fact]
    public void Application_ShouldFailToStart_WhenJwtSecretKeyIsMissing()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        // Clear all configuration sources and add empty in-memory config
                        config.Sources.Clear();
                        config.AddInMemoryCollection(new Dictionary<string, string?>());
                    });
                });

            // Trigger application startup
            _ = factory.CreateClient();
        });

        // Assert
        Assert.Contains("JWT_SECRET_KEY is not configured", exception.Message);
    }

    [Fact]
    public void Application_ShouldStart_WhenJwtSecretKeyIsConfigured()
    {
        // Arrange & Act - Use a custom factory that sets configuration before the app is built
        var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        // Assert - No exception should be thrown
        Assert.NotNull(client);
    }
}

// Custom factory for configuration validation tests
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:SecretKey", "test-secret-key-with-sufficient-length-for-hmac-sha256");
        builder.UseSetting("JWT_EXPIRATION_MINUTES", "60");
    }
}

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net;
using Xunit;

namespace API.Tests;

public class SwaggerConfigurationTests : IClassFixture<SwaggerTestWebApplicationFactory>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SwaggerConfigurationTests(SwaggerTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Swagger_Endpoint_Returns_Success()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Swagger_Json_Contains_Security_Definition()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Bearer", content);
        Assert.Contains("securitySchemes", content);
        Assert.Contains("JWT", content);
    }

    [Fact]
    public async Task Swagger_Json_Contains_Security_Requirement()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("security", content);
    }

    [Fact]
    public async Task Swagger_Json_Contains_Authentication_Instructions()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Authorization", content);
        Assert.Contains("obtain a token", content, StringComparison.OrdinalIgnoreCase);
    }
}

// Custom factory for Swagger configuration tests
public class SwaggerTestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:SecretKey", "test-secret-key-for-swagger-configuration-testing-minimum-32-characters");
        builder.UseSetting("JWT_EXPIRATION_MINUTES", "60");
        builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Development");
    }
}

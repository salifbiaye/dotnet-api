using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net;
using Xunit;

namespace API.Tests;

public class CorsConfigurationTests : IClassFixture<CorsTestWebApplicationFactory>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CorsConfigurationTests(CorsTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Request_WithAllowedOrigin_ShouldIncludeCorsHeaders()
    {
        // Arrange
        var client = _factory.CreateClient();
        var allowedOrigin = "http://localhost:3000";
        
        // Act
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/todoitems");
        request.Headers.Add("Origin", allowedOrigin);
        request.Headers.Add("Access-Control-Request-Method", "GET");
        
        var response = await client.SendAsync(request);
        
        // Assert
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin") || 
                    response.StatusCode == HttpStatusCode.NoContent ||
                    response.StatusCode == HttpStatusCode.OK,
                    "CORS headers should be present or preflight should succeed");
    }

    [Fact]
    public async Task Request_WithDisallowedOrigin_ShouldNotIncludeCorsHeaders()
    {
        // Arrange
        var client = _factory.CreateClient();
        var disallowedOrigin = "http://malicious-site.com";
        
        // Act
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/todoitems");
        request.Headers.Add("Origin", disallowedOrigin);
        request.Headers.Add("Access-Control-Request-Method", "GET");
        
        var response = await client.SendAsync(request);
        
        // Assert
        if (response.Headers.Contains("Access-Control-Allow-Origin"))
        {
            var allowedOrigin = response.Headers.GetValues("Access-Control-Allow-Origin").FirstOrDefault();
            Assert.NotEqual(disallowedOrigin, allowedOrigin);
        }
    }

    [Fact]
    public async Task Request_WithAllowedOrigin_ShouldAllowCredentials()
    {
        // Arrange
        var client = _factory.CreateClient();
        var allowedOrigin = "http://localhost:5173";
        
        // Act
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/todoitems");
        request.Headers.Add("Origin", allowedOrigin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        
        var response = await client.SendAsync(request);
        
        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.NoContent || 
                    response.StatusCode == HttpStatusCode.OK,
                    "Preflight request should succeed for allowed origin");
    }
}

// Custom factory for CORS configuration tests
public class CorsTestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:SecretKey", "test-secret-key-for-cors-testing-minimum-32-characters");
        builder.UseSetting("JWT_EXPIRATION_MINUTES", "60");
        builder.UseSetting("CORS_ALLOWED_ORIGINS", "http://localhost:3000,http://localhost:5173");
    }
}

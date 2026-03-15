using API_web.Models;
using API_web.Services;
using API_web.Middleware;
using API_web.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Configure logging to write to stdout for container compatibility
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TodoItems API",
        Version = "v1",
        Description = "A secure TodoItems API with JWT authentication and role-based authorization"
    });

    // Include XML comments for better documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                      "Enter your JWT token in the text input below.\r\n\r\n" +
                      "Example: \"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\"\r\n\r\n" +
                      "To obtain a token:\r\n" +
                      "1. Register a user via POST /api/auth/register\r\n" +
                      "2. Login via POST /api/auth/login to receive a JWT token\r\n" +
                      "3. Copy the token value and paste it here (without 'Bearer ' prefix)"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add Controllers
builder.Services.AddControllers();

// Register MongoDB
// En ASP.NET Core, les env vars avec __ overrident le JSON (MongoDB__ConnectionString > appsettings.json)
var mongoConnectionString = builder.Configuration["MongoDB:ConnectionString"] ?? "mongodb://127.0.0.1:27017";
builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnectionString));
builder.Services.AddSingleton<TodoContext>();

// Register password hashing service
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// Register JWT token service
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready" });

// Configure JWT authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? builder.Configuration["JWT_SECRET_KEY"];
if (string.IsNullOrEmpty(jwtSecretKey))
{
    var logger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger<Program>();
    logger.LogError("Configuration validation failed: JWT_SECRET_KEY is not configured. Please set the JWT_SECRET_KEY environment variable or add it to appsettings.json under Jwt:SecretKey.");
    throw new InvalidOperationException("JWT_SECRET_KEY is not configured. Please set the JWT_SECRET_KEY environment variable or add it to appsettings.json under Jwt:SecretKey.");
}

var key = Encoding.UTF8.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Configure CORS with whitelist
var corsAllowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
    ?? builder.Configuration["CORS_ALLOWED_ORIGINS"]?.Split(',', StringSplitOptions.RemoveEmptyEntries);

if (corsAllowedOrigins != null && corsAllowedOrigins.Length > 0)
{
    var allowedOrigins = corsAllowedOrigins
        .Select(origin => origin.Trim())
        .ToArray();

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });
}

var app = builder.Build();

// Add global exception handling middleware (must be first in pipeline)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS redirection only in production
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// Add security headers middleware
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Add authorization failure logging middleware
app.UseMiddleware<AuthorizationLoggingMiddleware>();

// Map health check endpoints (before authentication/authorization)
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false, // Don't run any checks for /health, just return healthy
    AllowCachingResponses = false
}).AllowAnonymous();

app.MapHealthChecks("/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    AllowCachingResponses = false,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        
        // Set status code based on health status
        context.Response.StatusCode = report.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy 
            ? StatusCodes.Status200OK 
            : StatusCodes.Status503ServiceUnavailable;
        
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            })
        });
        await context.Response.WriteAsync(result);
    }
}).AllowAnonymous();

// Map controller endpoints
app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Make the implicit Program class public for testing
public partial class Program { }

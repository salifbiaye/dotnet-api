using System.Security.Claims;

namespace API_web.Middleware;

/// <summary>
/// Middleware that logs authorization and authentication failures
/// </summary>
public class AuthorizationLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthorizationLoggingMiddleware> _logger;

    public AuthorizationLoggingMiddleware(RequestDelegate next, ILogger<AuthorizationLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        // Log 401 Unauthorized responses (invalid JWT token validation failures)
        if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
        {
            var endpoint = $"{context.Request.Method} {context.Request.Path}";
            var username = context.User?.Identity?.Name ?? "(anonymous)";
            
            // Check if user was authenticated but token was invalid
            if (context.User?.Identity?.IsAuthenticated == false && context.Request.Headers.ContainsKey("Authorization"))
            {
                _logger.LogWarning(
                    "JWT token validation failed for endpoint '{Endpoint}'. Invalid or expired token provided.",
                    endpoint);
            }
            else
            {
                _logger.LogWarning(
                    "Unauthorized access attempt by user '{Username}' to endpoint '{Endpoint}'. No valid authentication token provided.",
                    username,
                    endpoint);
            }
        }

        // Log 403 Forbidden responses (authorization failures)
        if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
        {
            var username = context.User?.Identity?.Name ?? "(unknown)";
            var endpoint = $"{context.Request.Method} {context.Request.Path}";
            var userRole = context.User?.FindFirst(ClaimTypes.Role)?.Value ?? "(no role)";
            
            // Extract required role from endpoint metadata if available
            var endpointMetadata = context.GetEndpoint()?.Metadata;
            var authorizeData = endpointMetadata?.GetMetadata<Microsoft.AspNetCore.Authorization.IAuthorizeData>();
            var requiredRoles = authorizeData?.Roles ?? "(unknown)";

            _logger.LogWarning(
                "Authorization failed: User '{Username}' with role '{UserRole}' attempted to access endpoint '{Endpoint}' which requires role(s) '{RequiredRoles}'",
                username,
                userRole,
                endpoint,
                requiredRoles);
        }
    }
}

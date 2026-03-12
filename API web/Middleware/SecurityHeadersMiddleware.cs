namespace API_web.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add Strict-Transport-Security header
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000");
        
        // Add X-Content-Type-Options header
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        
        // Add X-Frame-Options header
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        
        // Add X-XSS-Protection header
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        await _next(context);
    }
}

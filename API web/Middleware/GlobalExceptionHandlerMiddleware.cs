namespace API_web.Middleware;

/// <summary>
/// Middleware that catches and logs all unhandled exceptions
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Unhandled exception occurred while processing request {Method} {Path}. Exception: {ExceptionType}, Message: {Message}",
                context.Request.Method,
                context.Request.Path,
                ex.GetType().Name,
                ex.Message);

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var response = new
        {
            error = "An internal server error occurred.",
            message = _environment.IsDevelopment() ? exception.Message : "Please contact support if the problem persists.",
            type = exception.GetType().Name
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}

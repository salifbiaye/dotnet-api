using Microsoft.Extensions.Diagnostics.HealthChecks;
using API_web.Models;
using Microsoft.EntityFrameworkCore;

namespace API_web.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly TodoContext _context;
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);

    public DatabaseHealthCheck(TodoContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var timeoutCts = new CancellationTokenSource(_timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, timeoutCts.Token);

            // Try to connect to the database
            await _context.Database.CanConnectAsync(linkedCts.Token);

            return HealthCheckResult.Healthy("Database is accessible");
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("Database connection timed out after 5 seconds");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database is not accessible", ex);
        }
    }
}

using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace API_web.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IMongoClient _mongoClient;
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);

    public DatabaseHealthCheck(IMongoClient mongoClient)
    {
        _mongoClient = mongoClient;
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

            await _mongoClient.GetDatabase("admin")
                .RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: linkedCts.Token);

            return HealthCheckResult.Healthy("MongoDB is accessible");
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("MongoDB connection timed out after 5 seconds");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("MongoDB is not accessible", ex);
        }
    }
}

using GuardianPet.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GuardianPet.HealthChecks;

public sealed class OracleHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;

    public OracleHealthCheck(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("Oracle connection is available.")
                : HealthCheckResult.Unhealthy("Oracle connection is unavailable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Oracle connection failed.", exception);
        }
    }
}

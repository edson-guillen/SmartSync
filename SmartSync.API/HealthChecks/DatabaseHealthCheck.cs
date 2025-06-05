using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartSync.Infraestructure.Persistence.Context;

namespace SmartSync.API.HealthChecks
{
    public class DatabaseHealthCheck(SmartSyncDbContext dbContext) : IHealthCheck
    {
        private readonly SmartSyncDbContext _dbContext = dbContext;

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Verifica se consegue conectar ao banco
                var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

                if (!canConnect)
                {
                    return HealthCheckResult.Unhealthy("Database connection failed");
                }

                // Verifica se consegue fazer uma query simples
                var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
                await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1", timeoutToken);

                return HealthCheckResult.Healthy("Database connection is healthy");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Database health check failed", ex);
            }
        }
    }
}
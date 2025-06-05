using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SmartSync.Infraestructure.Messaging;

namespace SmartSync.API.HealthChecks
{
    public class RabbitMqHealthCheck(IOptions<RabbitMqOptions> options) : IHealthCheck
    {
        private readonly RabbitMqOptions _options = options.Value;

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(_options.ConnectionString),
                    RequestedHeartbeat = TimeSpan.FromSeconds(30)
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                return Task.FromResult(HealthCheckResult.Healthy("RabbitMQ connection is healthy"));
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ connection failed", ex));
            }
        }
    }
}
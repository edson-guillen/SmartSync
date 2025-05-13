using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using SmartSync.Infraestructure.Messaging.Config;

namespace SmartSync.Infraestructure.Messaging.Queueing
{
    public static class QueueInitializer
    {
        public static void RegisterQueues(IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            var rabbitMqUrl = configuration["RabbitMq:Host"];
            if (string.IsNullOrWhiteSpace(rabbitMqUrl))
                throw new Exception("RabbitMQ connection string not found in configuration.");

            var factory = new ConnectionFactory
            {
                Uri = new Uri(rabbitMqUrl)
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            var configService = new QueueConfigurationService(channel);

            // Registrar a fila de acender luzes (Fanout + DLQ + TTL)
            configService.DeclareQueueWithDLQ(new QueueConfiguration
            {
                QueueName = "smart.sync.acender.comodo",
                ExchangeName = "smart.sync.acender.fanout",
                MessageTtl = 30000, // 30 segundos
                Durable = true,
                AutoDelete = false,
                Exclusive = false
            });

            // Você pode registrar outras filas aqui futuramente...
        }
    }
}

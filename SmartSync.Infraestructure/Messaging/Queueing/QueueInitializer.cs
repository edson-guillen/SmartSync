using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartSync.Infraestructure.Logging.Interfaces;
using SmartSync.Infraestructure.Messaging.Config;
using SmartSync.Infraestructure.Messaging.Consumers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Messaging.Queueing
{
    public class QueueInitializer : IHostedService
    {
        private readonly QueueConfigurationService _queueConfigurationService;
        private readonly IMessageLogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public QueueInitializer(
            QueueConfigurationService queueConfigurationService,
            IMessageLogger logger,
            IServiceProvider serviceProvider)
        {
            _queueConfigurationService = queueConfigurationService ?? throw new ArgumentNullException(nameof(queueConfigurationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInfo("Inicializando filas RabbitMQ...");

            try
            {
                // Define constantes para evitar inconsistências de string
                const string queueName = "smartsync.comodo.acender";
                const string exchangeName = "smartsync.comodo.acender.fanout";
                const string deadLetterExchange = "smartsync.comodo.acender.dlx";
                const int messageTtl = 30000;

                // Configure your queues here
                _queueConfigurationService.DeclareQueueWithDLQ(
                    queueName: queueName,
                    exchangeName: exchangeName,
                    deadLetterExchange: deadLetterExchange,
                    messageTtl: messageTtl);

                // Add more queue declarations as needed

                _logger.LogInfo("Filas RabbitMQ inicializadas com sucesso");

                // Encontrar todas as instâncias de consumidores que precisam ser notificadas
                var consumers = GetConsumers();
                foreach (var consumer in consumers)
                {
                    // Chamada do método de notificação para cada consumidor encontrado
                    consumer?.NotifyQueuesInitialized();
                    _logger.LogInfo($"Consumidor {consumer?.GetType().Name} notificado sobre a inicialização das filas");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Falha ao inicializar filas RabbitMQ", ex);
                throw; // Rethrow to prevent application startup
            }

            // Aguarda um momento para garantir que as filas foram criadas antes dos consumidores serem iniciados
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }

        private IEnumerable<AcenderLuzesComodoConsumer> GetConsumers()
        {
            // Move the try-catch block outside of the iterator method to avoid the CS1626 error  
            List<AcenderLuzesComodoConsumer> consumers = new List<AcenderLuzesComodoConsumer>();

            try
            {
                // Tenta obter todos os consumidores registrados  
                var hostedServices = _serviceProvider.GetServices<IHostedService>();
                foreach (var service in hostedServices)
                {
                    if (service is AcenderLuzesComodoConsumer consumer)
                    {
                        consumers.Add(consumer);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao tentar obter consumidores", ex);
            }

            return consumers;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
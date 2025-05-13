using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartSync.Domain.Events;
using SmartSync.Infraestructure.Logging.Interfaces;
using SmartSync.Infraestructure.Messaging.Middleware;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SmartSync.Infraestructure.Messaging.Handler;

namespace SmartSync.Infraestructure.Messaging.Consumers
{
    public class AcenderLuzesComodoConsumer : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly IMessageLogger _logger;
        private readonly RetryHandler _retryHandler;
        private readonly IModel _channel;
        private const string QUEUE_NAME = "smartsync.comodo.acender";
        private readonly SemaphoreSlim _initializationSemaphore = new SemaphoreSlim(0, 1);

        public AcenderLuzesComodoConsumer(
            IServiceProvider provider,
            IMessageLogger logger,
            RetryHandler retryHandler,
            IModel channel)
        {
            _provider = provider;
            _logger = logger;
            _retryHandler = retryHandler;
            _channel = channel;
        }

        /// <summary>
        /// Este método deve ser chamado pelo QueueInitializer após a criação das filas
        /// </summary>
        public void NotifyQueuesInitialized()
        {
            _initializationSemaphore.Release();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInfo($"Aguardando inicialização das filas antes de configurar o consumidor para '{QUEUE_NAME}'");

            // Aguarda até 30 segundos pela inicialização das filas
            // Caso o semáforo não seja liberado nesse tempo, continua mesmo assim
            await _initializationSemaphore.WaitAsync(TimeSpan.FromSeconds(30), stoppingToken);

            // Verifica se a fila existe antes de tentar consumir
            try
            {
                // Verifica se a fila existe
                _channel.QueueDeclarePassive(QUEUE_NAME);
                _logger.LogInfo($"Fila '{QUEUE_NAME}' encontrada, configurando consumidor");
            }
            catch (Exception ex)
            {
                _logger.LogError($"A fila '{QUEUE_NAME}' não foi encontrada. Certifique-se de que ela foi criada corretamente pelo QueueInitializer", ex);
                // Tenta criar a fila se ela não existir
                try
                {
                    var arguments = new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "x-dead-letter-exchange", "smartsync.comodo.acender.dlx" },
                        { "x-message-ttl", 30000 }
                    };

                    _channel.ExchangeDeclare("smartsync.comodo.acender.fanout", ExchangeType.Fanout, durable: true);
                    _channel.ExchangeDeclare("smartsync.comodo.acender.dlx", ExchangeType.Fanout, durable: true);
                    _channel.QueueDeclare(QUEUE_NAME, durable: true, exclusive: false, autoDelete: false, arguments);
                    _channel.QueueBind(QUEUE_NAME, "smartsync.comodo.acender.fanout", "");

                    _logger.LogInfo($"Fila '{QUEUE_NAME}' criada com sucesso pelo consumidor");
                }
                catch (Exception createEx)
                {
                    _logger.LogError($"Falha ao criar a fila '{QUEUE_NAME}' no consumidor", createEx);
                    throw;
                }
            }

            // Configura o consumidor
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (sender, ea) =>
            {
                try
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    _logger.LogInfo($"Mensagem recebida: {message}");
                    var evento = JsonSerializer.Deserialize<AcenderLuzesComodoEvent>(message);

                    await _retryHandler.ExecuteWithRetryAsync(async () =>
                    {
                        using var scope = _provider.CreateScope();
                        var handler = scope.ServiceProvider.GetRequiredService<AcenderLuzesComodoEventHandler>();
                        await handler.HandleAsync(evento!);
                        _channel.BasicAck(ea.DeliveryTag, false);
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError("Erro ao processar mensagem", ex);
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            _channel.BasicConsume(QUEUE_NAME, false, consumer);
            _logger.LogInfo($"Consumidor configurado para a fila '{QUEUE_NAME}'");
        }
    }
}
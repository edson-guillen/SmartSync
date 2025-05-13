using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartSync.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using SmartSync.Infraestructure.Messaging.Config;
using Microsoft.Extensions.DependencyInjection;
using SmartSync.Infraestructure.Messaging.Handler;
using SmartSync.Infraestructure.Messaging.Utils;

namespace SmartSync.Infraestructure.Messaging.Consumers
{
    public class AcenderLuzesComodoConsumer : BackgroundService
    {
        private readonly ILogger<AcenderLuzesComodoConsumer> _logger;
        private readonly IServiceProvider _provider;
        private readonly RabbitMqSettings _settings;

        public AcenderLuzesComodoConsumer(
            ILogger<AcenderLuzesComodoConsumer> logger,
            IOptions<RabbitMqSettings> options,
            IServiceProvider provider)
        {
            _logger = logger;
            _provider = provider;
            _settings = options.Value;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                DispatchConsumersAsync = true
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.ExchangeDeclare(_settings.ExchangeName, ExchangeType.Fanout, durable: true);
            channel.ExchangeDeclare(_settings.DeadLetterExchange, ExchangeType.Fanout, durable: true);

            channel.QueueDeclare("smartsync.comodo.acender", durable: true, exclusive: false, autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                { "x-dead-letter-exchange", _settings.DeadLetterExchange }
                });

            channel.QueueBind("smartsync.comodo.acender", _settings.ExchangeName, "");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (sender, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var evento = JsonSerializer.Deserialize<AcenderLuzesComodoEvent>(json);

                    using var scope = _provider.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<AcenderLuzesComodoEventHandler>();

                    await handler.HandleAsync(evento!);

                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Consumer] Erro ao processar mensagem");

                    var retryCount = RetryPolicyHelper.GetRetryCount(ea.BasicProperties);
                    if (retryCount < 5)
                    {
                        RetryPolicyHelper.AddRetryHeader(ea.BasicProperties, retryCount + 1);
                        channel.BasicPublish(exchange: _settings.ExchangeName,
                                             routingKey: "",
                                             basicProperties: ea.BasicProperties,
                                             body: ea.Body);
                    }
                    else
                    {
                        channel.BasicPublish(_settings.DeadLetterExchange, "", null, ea.Body);
                    }

                    channel.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            channel.BasicConsume("smartsync.comodo.acender", false, consumer);

            return Task.CompletedTask;
        }
    }
}

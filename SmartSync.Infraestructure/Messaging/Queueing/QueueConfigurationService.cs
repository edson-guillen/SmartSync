using RabbitMQ.Client;
using SmartSync.Infraestructure.Messaging.Config;
using System;
using System.Collections.Generic;

namespace SmartSync.Infraestructure.Messaging.Queueing
{
    public class QueueConfigurationService
    {
        private readonly IModel _channel;

        public QueueConfigurationService(IModel channel)
        {
            _channel = channel;
        }

        public void DeclareQueueWithDLQ(QueueConfiguration config)
        {
            if (string.IsNullOrEmpty(config.QueueName) || string.IsNullOrEmpty(config.ExchangeName))
                throw new ArgumentException("QueueName and ExchangeName are required.");

            // 1. Criar a DLQ (Dead Letter Queue)
            var deadLetterQueue = $"{config.QueueName}.dlq";
            var deadLetterExchange = $"{config.ExchangeName}.dlx";

            _channel.ExchangeDeclare(exchange: deadLetterExchange, type: ExchangeType.Fanout, durable: true);
            _channel.QueueDeclare(
                queue: deadLetterQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            _channel.QueueBind(queue: deadLetterQueue, exchange: deadLetterExchange, routingKey: string.Empty);

            // 2. Criar a fila principal com DLQ e TTL
            var arguments = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", deadLetterExchange }
            };

            if (config.MessageTtl > 0)
                arguments.Add("x-message-ttl", config.MessageTtl);

            _channel.ExchangeDeclare(exchange: config.ExchangeName, type: ExchangeType.Fanout, durable: true);

            _channel.QueueDeclare(
                queue: config.QueueName,
                durable: config.Durable,
                exclusive: config.Exclusive,
                autoDelete: config.AutoDelete,
                arguments: arguments
            );

            _channel.QueueBind(queue: config.QueueName, exchange: config.ExchangeName, routingKey: config.RoutingKey ?? "");
        }
    }
}

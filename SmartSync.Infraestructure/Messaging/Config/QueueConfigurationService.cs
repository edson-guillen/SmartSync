using RabbitMQ.Client;
using SmartSync.Infraestructure.Logging.Interfaces;
using System;
using System.Collections.Generic;

namespace SmartSync.Infraestructure.Messaging.Config
{
    public class QueueConfigurationService
    {
        private readonly IModel _channel;
        private readonly IMessageLogger _logger;

        public QueueConfigurationService(IModel channel, IMessageLogger logger)
        {
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void DeclareQueueWithDLQ(string queueName, string exchangeName, string deadLetterExchange, int messageTtl)
        {
            if (string.IsNullOrWhiteSpace(queueName) || string.IsNullOrWhiteSpace(exchangeName) || string.IsNullOrWhiteSpace(deadLetterExchange))
                throw new ArgumentException("Queue name, exchange name, and DLX cannot be null or empty.");

            try
            {
                // Declare Dead Letter Exchange
                _channel.ExchangeDeclare(deadLetterExchange, ExchangeType.Fanout, durable: true);

                // Declare Fanout Exchange
                _channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout, durable: true);

                // Check if queue exists first
                try
                {
                    // This will throw an exception if the queue doesn't exist
                    _channel.QueueDeclarePassive(queueName);

                    // If we reach here, the queue exists
                    _logger.LogInfo($"Queue '{queueName}' already exists. Skipping queue declaration to avoid property conflicts.");

                    // Make sure the binding is present, even if queue is already defined
                    _channel.QueueBind(queueName, exchangeName, "");
                }
                catch (RabbitMQ.Client.Exceptions.OperationInterruptedException)
                {
                    // Queue doesn't exist, create it with desired properties
                    _logger.LogInfo($"Creating queue '{queueName}' with TTL {messageTtl}ms and DLX '{deadLetterExchange}'");

                    var arguments = new Dictionary<string, object>
                    {
                        { "x-dead-letter-exchange", deadLetterExchange },
                        { "x-message-ttl", messageTtl }
                    };

                    _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false, arguments);
                    _channel.QueueBind(queueName, exchangeName, "");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error configuring queue '{queueName}'", ex);
                throw;
            }
        }
    }
}
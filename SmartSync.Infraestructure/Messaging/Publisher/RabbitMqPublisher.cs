using RabbitMQ.Client;
using SmartSync.Infraestructure.Messaging.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Messaging.Publisher
{
    public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        private readonly string _mainExchange = "benny.main-exchange";
        private readonly string _retryExchange = "benny.retry-exchange";
        private readonly string _mainQueue = "benny";
        private readonly string _retryQueue = "benny.retry";

        public RabbitMqPublisher(RabbitMqOptions options)
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(options.ConnectionString)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Ativa confirmações de publicação
            _channel.ConfirmSelect();

            // Declara exchanges
            _channel.ExchangeDeclare(_mainExchange, ExchangeType.Direct, durable: true);
            _channel.ExchangeDeclare(_retryExchange, ExchangeType.Direct, durable: true);

            // Declara fila principal com DLX para retryExchange
            _channel.QueueDeclare(_mainQueue, durable: true, exclusive: false, autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", _retryExchange }
                });
            _channel.QueueBind(_mainQueue, _mainExchange, routingKey: "");

            // Declara fila retry com TTL e DLX para mainExchange
            _channel.QueueDeclare(_retryQueue, durable: true, exclusive: false, autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    { "x-message-ttl", 10000 }, // espera 10s antes de tentar reenviar
                    { "x-dead-letter-exchange", _mainExchange }
                });
            _channel.QueueBind(_retryQueue, _retryExchange, routingKey: "");
        }

        public Task PublishAsync<T>(string queue, T message)
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            // Usa a exchange principal fixa
            _channel.BasicPublish(
                exchange: _mainExchange,
                routingKey: "",
                basicProperties: properties,
                body: body
            );

            // Espera confirmação de que a mensagem foi publicada
            _channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}

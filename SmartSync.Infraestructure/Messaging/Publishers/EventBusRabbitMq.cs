using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartSync.Infraestructure.Messaging.Config;
using SmartSync.Infraestructure.Messaging.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Messaging.Publishers
{
    public class EventBusRabbitMq : IEventBus
    {
        private readonly RabbitMqSettings _settings;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public EventBusRabbitMq(IOptions<RabbitMqSettings> options)
        {
            _settings = options.Value;

            var factory = new ConnectionFactory()
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Exchanges
            _channel.ExchangeDeclare(_settings.ExchangeName, ExchangeType.Fanout, durable: true);
            _channel.ExchangeDeclare(_settings.DeadLetterExchange, ExchangeType.Fanout, durable: true);
            _channel.ExchangeDeclare(_settings.RetryExchange, ExchangeType.Fanout, durable: true);
        }

        public void Publish<T>(T @event) where T : class
        {
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: _settings.ExchangeName, routingKey: "", body: body);
        }
    }
}

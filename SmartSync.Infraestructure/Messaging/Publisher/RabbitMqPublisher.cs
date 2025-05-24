using RabbitMQ.Client;
using SmartSync.Infraestructure.Messaging.Interfaces;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Messaging.Publisher
{
    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqPublisher(RabbitMqOptions options)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public Task PublishAsync<T>(string queue, T message)
        {
            _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;

            _channel.BasicPublish(
                exchange: "",
                routingKey: queue,
                basicProperties: properties,
                body: body
            );

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
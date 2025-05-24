using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartSync.Infraestructure.Messaging.Interfaces;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Messaging.Subscriber
{
    public class RabbitMqSubscriber : IRabbitMqSubscriber
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly Dictionary<string, Delegate> _handlers = new();
        private readonly List<EventingBasicConsumer> _consumers = new();
        private readonly RabbitMqOptions _options;
        private bool _running = false;

        public RabbitMqSubscriber(RabbitMqOptions options)
        {
            _options = options;
            var factory = new ConnectionFactory { HostName = "localhost" };

            //if (!string.IsNullOrWhiteSpace(options.ConnectionString))
            //    factory.Uri = new Uri(options.ConnectionString);
            //else
            //{
            //    factory.HostName = options.HostName;
            //    factory.UserName = options.UserName;
            //    factory.Password = options.Password;
            //    factory.VirtualHost = options.VirtualHost;
            //    factory.Port = options.Port;
            //}

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void Subscribe<T>(string queue, Func<T, Task> onMessage)
        {
            _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
            _handlers[queue] = onMessage;
        }

        public void Start()
        {
            _running = true;
            foreach (var kv in _handlers)
            {
                var queue = kv.Key;
                var handler = kv.Value;

                var consumer = new EventingBasicConsumer(_channel);
                object message = null;
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    try
                    {
                        message = json;
                    }
                    catch (Exception ex)
                    {
                        // Log do erro de deserialização
                        Console.WriteLine($"Erro ao deserializar mensagem da fila '{queue}': {ex.Message}");
                        // Opcional: Nack ou rejeitar a mensagem
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                        return;
                    }
                    await (Task)handler.DynamicInvoke(message);

                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };
                _channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
                _consumers.Add(consumer);
            }
        }

        public void Stop()
        {
            _running = false;
            foreach (var consumer in _consumers)
            {
            }
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
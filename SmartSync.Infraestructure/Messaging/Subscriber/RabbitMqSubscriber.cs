using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartSync.Infraestructure.Messaging.Interfaces;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Messaging.Subscriber
{
    public class RabbitMqSubscriber : IRabbitMqSubscriber, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly Dictionary<string, Func<string, Task>> _handlers = new();
        private readonly List<EventingBasicConsumer> _consumers = new();
        private readonly RabbitMqOptions _options;
        private bool _running = false;
        public IModel Channel => _channel;

        public RabbitMqSubscriber(RabbitMqOptions options)
        {
            _options = options;
            var factory = new ConnectionFactory
            {
                Uri = new Uri(_options.ConnectionString)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void Subscribe<T>(string queue, Func<T, Task> onMessage, int retryDelayMs = 10000, int maxRetries = 3)
        {
            var retryQueue = $"{queue}.retry";
            var retryExchange = $"{queue}.retry-exchange";
            var mainExchange = $"{queue}.main-exchange";

            // Exchanges
            _channel.ExchangeDeclare(mainExchange, ExchangeType.Direct, durable: true);
            _channel.ExchangeDeclare(retryExchange, ExchangeType.Direct, durable: true);

            // Fila principal
            _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false, arguments: new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", retryExchange }
            });
            _channel.QueueBind(queue, mainExchange, routingKey: "");

            // Fila de retry com TTL
            _channel.QueueDeclare(retryQueue, durable: true, exclusive: false, autoDelete: false, arguments: new Dictionary<string, object>
            {
                { "x-message-ttl", retryDelayMs },
                { "x-dead-letter-exchange", mainExchange }
            });
            _channel.QueueBind(retryQueue, retryExchange, routingKey: "");

            // Handler com JSON deserialization
            _handlers[queue] = async (json) =>
            {
                var message = JsonSerializer.Deserialize<T>(json);
                await onMessage(message);
            };
        }

        public void SubscribeToFanout<T>(string exchange, string queue, Func<T, Task> onMessage)
        {
            _channel.ExchangeDeclare(exchange, ExchangeType.Fanout, durable: true);
            _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue, exchange, routingKey: "");

            _handlers[queue] = async (json) =>
            {
                var message = JsonSerializer.Deserialize<T>(json);
                await onMessage(message);
            };
        }

        public void Start(int maxRetries = 3)
        {
            _running = true;

            foreach (var kv in _handlers)
            {
                var queue = kv.Key;
                var handler = kv.Value;

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);

                    int retryCount = 0;
                    if (ea.BasicProperties?.Headers != null && ea.BasicProperties.Headers.ContainsKey("x-retries"))
                    {
                        var raw = ea.BasicProperties.Headers["x-retries"];

                        if (raw is byte[] bytes)
                        {
                            var str = Encoding.UTF8.GetString(bytes);
                            int.TryParse(str, out retryCount);
                        }
                        else if (raw is int i)
                        {
                            retryCount = i;
                        }
                        else if (raw is long l)
                        {
                            retryCount = (int)l;
                        }
                        else if (raw != null)
                        {
                            int.TryParse(raw.ToString(), out retryCount);
                        }
                    }

                    try
                    {
                        await handler(json);
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[!] Erro ao processar mensagem da fila '{queue}': {ex.Message}");

                        if (retryCount >= maxRetries)
                        {
                            Console.WriteLine($"[!] Máximo de {maxRetries} tentativas atingido. Descartando mensagem.");
                            _channel.BasicNack(ea.DeliveryTag, false, false); // Dropa
                            return;
                        }

                        // Reenvia para retry
                        var retryProps = _channel.CreateBasicProperties();
                        retryProps.Persistent = true;
                        retryProps.Headers = new Dictionary<string, object>
                        {
                            { "x-retries", (retryCount + 1).ToString() }
                        };

                        _channel.BasicPublish(
                            exchange: $"{queue}.retry-exchange",
                            routingKey: "",
                            basicProperties: retryProps,
                            body: body
                        );

                        _channel.BasicAck(ea.DeliveryTag, false); // Marca como processada
                    }
                };

                _channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
                _consumers.Add(consumer);
            }
        }

        public void Stop()
        {
            _running = false;
            // Poderia dar cancel nos consumers, se quisesse
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}

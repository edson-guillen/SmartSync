using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartSync.Infraestructure.Messaging.Interfaces;
using System.Text;
using System.Text.Json;
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

        
        private const string DlxExchangeName = "dlx_exchange";
        private const string DlqQueueName = "dead_letter_queue";
        private const string DlqRoutingKey = "dlq_routing_key";

        public RabbitMqSubscriber(RabbitMqOptions options)
        {
            _options = options;
            var factory = new ConnectionFactory { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            
            _channel.ExchangeDeclare(DlxExchangeName, ExchangeType.Direct, durable: true);
            _channel.QueueDeclare(DlqQueueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(DlqQueueName, DlxExchangeName, DlqRoutingKey);
        }

        public void Subscribe<T>(string queue, Func<T, Task> onMessage)
        {
            
            var mainQueueArgs = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", DlxExchangeName },
                { "x-dead-letter-routing-key", DlqRoutingKey }
            };

            _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false, arguments: mainQueueArgs);
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
                        
                        Console.WriteLine($"Erro ao deserializar mensagem da fila '{queue}': {ex.Message}");
                        
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                        return;
                    }

                    try
                    {
                        await (Task)handler.DynamicInvoke(message);
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        
                        Console.WriteLine($"Erro no processamento da mensagem da fila '{queue}': {ex.Message}");
                        
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                    }
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

using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartSync.Domain.Events;
using SmartSync.Infraestructure.Persistence.Interfaces;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace SmartSync.Infraestructure.Messaging.Subscriber
{
    public class ResidenciaFanoutSubscriber : BackgroundService
    {
        private readonly IModel _channel;
        private readonly Guid _residenciaId;
        private readonly string _exchangeName;
        private readonly string _queueName;
        private readonly IComodoRepository _comodoRepository;

        public ResidenciaFanoutSubscriber(IModel channel, Guid residenciaId, IComodoRepository comodoRepository)
        {
            _channel = channel;
            _residenciaId = residenciaId;
            _exchangeName = $"residencia.{_residenciaId}";
            _queueName = $"residencia.{_residenciaId}.fanout.subscriber";
            _comodoRepository = comodoRepository;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _channel.ExchangeDeclare(_exchangeName, ExchangeType.Fanout, durable: true);
            _channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(_queueName, _exchangeName, routingKey: "");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var acao = JsonSerializer.Deserialize<AcaoCommand>(json);
                Console.WriteLine($"[ResidenciaFanoutSubscriber] Mensagem recebida na exchange '{_exchangeName}': {json}");

                // Replicar para todas as exchanges dos cômodos da residência
                var comodos = _comodoRepository.Get(c => c.ResidenciaId == _residenciaId).ToList();
                foreach (var comodo in comodos)
                {
                    var exchangeComodo = $"comodo.{comodo.Id}";
                    _channel.BasicPublish(
                        exchange: exchangeComodo,
                        routingKey: "",
                        basicProperties: null,
                        body: body
                    );
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }
    }
}

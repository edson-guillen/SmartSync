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
    public class ComodoFanoutSubscriber : BackgroundService
    {
        private readonly IModel _channel;
        private readonly Guid _comodoId;
        private readonly string _exchangeName;
        private readonly string _queueName;
        private readonly IDispositivoRepository _dispositivoRepository;

        public ComodoFanoutSubscriber(IModel channel, Guid comodoId, IDispositivoRepository dispositivoRepository)
        {
            _channel = channel;
            _comodoId = comodoId;
            _exchangeName = $"comodo.{_comodoId}";
            _queueName = $"comodo.{_comodoId}.fanout.subscriber";
            _dispositivoRepository = dispositivoRepository;
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
                Console.WriteLine($"[ComodoFanoutSubscriber] Mensagem recebida na exchange '{_exchangeName}': {json}");

                // Envia para cada dispositivo do cômodo
                var dispositivos = _dispositivoRepository.Get(d => d.ComodoId == _comodoId).ToList();
                foreach (var dispositivo in dispositivos)
                {
                    var filaDispositivo = $"dispositivo.{dispositivo.Id}";
                    _channel.BasicPublish(
                        exchange: "",
                        routingKey: filaDispositivo,
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

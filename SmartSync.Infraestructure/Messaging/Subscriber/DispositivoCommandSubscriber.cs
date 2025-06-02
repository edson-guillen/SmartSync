using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartSync.Domain.Entities;
using SmartSync.Domain.Events;
using SmartSync.Infraestructure.Persistence.Interfaces;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Messaging.Subscriber
{
    public class DispositivoCommandSubscriber : BackgroundService
    {
        private readonly IModel _channel;
        private readonly IServiceScopeFactory _scopeFactory;

        public DispositivoCommandSubscriber(
            IModel channel,
            IServiceScopeFactory scopeFactory)
        {
            _channel = channel;
            _scopeFactory = scopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dispositivoRepository = scope.ServiceProvider.GetRequiredService<IDispositivoRepository>();

            var dispositivos = dispositivoRepository.Get().ToList();
            foreach (var dispositivo in dispositivos)
            {
                var queue = $"dispositivo.{dispositivo.Id}";

                // Não declare a fila novamente, apenas consuma
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var msg = JsonSerializer.Deserialize<AcaoCommand>(json);

                    using var innerScope = _scopeFactory.CreateScope();
                    var repo = innerScope.ServiceProvider.GetRequiredService<IDispositivoRepository>();

                    var acao = (string)msg?.Acao;
                    if (acao == "Ligar")
                    {
                        dispositivo.Ligar();
                        await repo.Update(dispositivo);
                        Console.WriteLine($"[Dispositivo {dispositivo.Id}] Ligado via fila.");
                    }
                    else if (acao == "Desligar")
                    {
                        dispositivo.Desligar();
                        await repo.Update(dispositivo);
                        Console.WriteLine($"[Dispositivo {dispositivo.Id}] Desligado via fila.");
                    }
                    else
                    {
                        Console.WriteLine($"[Dispositivo {dispositivo.Id}] Ação desconhecida: {acao}");
                    }

                    _channel.BasicAck(ea.DeliveryTag, false);
                };

                _channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
            }

            return Task.CompletedTask;
        }

    }
}

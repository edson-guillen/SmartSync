using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using SmartSync.Domain.Entities;
using SmartSync.Domain.Events;
using SmartSync.Infraestructure.Messaging.Interfaces;
using SmartSync.Infraestructure.Persistence.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Messaging.Subscriber
{
    public class DispositivoCommandSubscriber : BackgroundService
    {
        private readonly IRabbitMqSubscriber _subscriber;
        private readonly IServiceScopeFactory _scopeFactory;

        public DispositivoCommandSubscriber(
            IRabbitMqSubscriber subscriber,
            IServiceScopeFactory scopeFactory)
        {
            _subscriber = subscriber;
            _scopeFactory = scopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dispositivoRepository = scope.ServiceProvider.GetRequiredService<IDispositivoRepository>();
            var comodoRepository = scope.ServiceProvider.GetRequiredService<IComodoRepository>();
            var residenciaRepository = scope.ServiceProvider.GetRequiredService<IResidenciaRepository>();

            // Declara e faz bind das exchanges de residências para cômodos
            var residencias = residenciaRepository.Get().ToList();
            foreach (var residencia in residencias)
            {
                string residenciaExchange = $"residencia.{residencia.Id}";
                var channel = (_subscriber as SmartSync.Infraestructure.Messaging.Subscriber.RabbitMqSubscriber)?.Channel;
                channel?.ExchangeDeclare(residenciaExchange, ExchangeType.Fanout, durable: true);

                var comodosDaResidencia = comodoRepository.Get(c => c.ResidenciaId == residencia.Id).ToList();
                foreach (var comodo in comodosDaResidencia)
                {
                    string comodoExchange = $"comodo.{comodo.Id}";
                    channel?.ExchangeDeclare(comodoExchange, ExchangeType.Fanout, durable: true);
                    channel?.ExchangeBind(destination: comodoExchange, source: residenciaExchange, routingKey: "");
                }
            }

            // Não declare nem faça bind manual das filas replicator aqui!

            // Inscreve em todas as filas de dispositivos existentes
            var dispositivos = dispositivoRepository.Get().ToList();
            foreach (var dispositivo in dispositivos)
            {
                var queue = $"dispositivo.{dispositivo.Id}";
                _subscriber.Subscribe<AcaoCommand>(queue, async (msg) =>
                {
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
                });
            }

            _subscriber.Start();
            return Task.CompletedTask;
        }
    }
}

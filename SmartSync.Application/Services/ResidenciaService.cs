using RabbitMQ.Client;
using SmartSync.Application.Interfaces;
using SmartSync.Domain.Entities;
using SmartSync.Domain.Events;
using SmartSync.Infraestructure.Messaging.Interfaces;
using SmartSync.Infraestructure.Persistence.Interfaces;
using System;
using System.Threading.Tasks;

namespace SmartSync.Application.Services
{
    public class ResidenciaService(
        IResidenciaRepository repository,
        IRabbitMqPublisher publisher
    ) : BaseService<Residencia>(repository, publisher), IResidenciaService
    {
        public override async Task<int> Insert(Residencia residencia)
        {
            var result = await base.Insert(residencia);

            if (publisher is SmartSync.Infraestructure.Messaging.Publisher.RabbitMqPublisher concretePublisher)
            {
                var channel = concretePublisher.Channel;
                string exchange = $"residencia.{residencia.Id}";
                channel.ExchangeDeclare(exchange, ExchangeType.Fanout, durable: true);
            }

            return result;
        }

        public override async Task<int> Delete(Guid id)
        {
            // Remove a exchange da residência
            if (publisher is SmartSync.Infraestructure.Messaging.Publisher.RabbitMqPublisher concretePublisher)
            {
                var channel = concretePublisher.Channel;
                string exchange = $"residencia.{id}";
                channel.ExchangeDelete(exchange, ifUnused: false);
            }
            return await base.Delete(id);
        }

        public async Task LigarTodosDispositivos(Guid residenciaId)
        {
            await publisher.PublishToFanoutAsync($"residencia.{residenciaId}", new AcaoCommand { ResidenciaId = residenciaId, Acao = "Ligar" });
        }

        public async Task DesligarTodosDispositivos(Guid residenciaId)
        {
            await publisher.PublishToFanoutAsync($"residencia.{residenciaId}", new AcaoCommand { ResidenciaId = residenciaId, Acao = "Desligar" });
        }
    }
}

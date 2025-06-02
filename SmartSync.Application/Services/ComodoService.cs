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
    public class ComodoService(
        IComodoRepository repository,
        IRabbitMqPublisher publisher,
        IRabbitMqSubscriber subscriber
    ) : BaseService<Comodo>(repository, publisher), IComodoService
    {
        private readonly IRabbitMqSubscriber _subscriber = subscriber;

        public override async Task<int> Insert(Comodo comodo)
        {
            var result = await base.Insert(comodo);
            DeclareComodoExchange(comodo.Id);
            BindComodoToResidenciaExchange(comodo.ResidenciaId, comodo.Id);
            return result;
        }

        public override async Task<int> Delete(Guid id)
        {
            // Remove a exchange do cômodo
            if (_subscriber is SmartSync.Infraestructure.Messaging.Subscriber.RabbitMqSubscriber rabbitSubscriber)
            {
                var channel = rabbitSubscriber.Channel;
                string exchange = $"comodo.{id}";
                channel.ExchangeDelete(exchange, ifUnused: false);
            }
            return await base.Delete(id);
        }

        public async Task LigarTodosDispositivos(Guid comodoId)
        {
            DeclareComodoExchange(comodoId);
            await publisher.PublishToFanoutAsync($"comodo.{comodoId}", new AcaoCommand { ComodoId = comodoId, Acao = "Ligar" });
        }

        public async Task DesligarTodosDispositivos(Guid comodoId)
        {
            DeclareComodoExchange(comodoId);
            await publisher.PublishToFanoutAsync($"comodo.{comodoId}", new AcaoCommand { ComodoId = comodoId, Acao = "Desligar" });
        }

        private void DeclareComodoExchange(Guid comodoId)
        {
            if (_subscriber is SmartSync.Infraestructure.Messaging.Subscriber.RabbitMqSubscriber rabbitSubscriber)
            {
                var channel = rabbitSubscriber.Channel;
                string comodoExchange = $"comodo.{comodoId}";
                channel.ExchangeDeclare(comodoExchange, ExchangeType.Fanout, durable: true);
            }
        }

        private void BindComodoToResidenciaExchange(Guid residenciaId, Guid comodoId)
        {
            if (_subscriber is SmartSync.Infraestructure.Messaging.Subscriber.RabbitMqSubscriber rabbitSubscriber)
            {
                var channel = rabbitSubscriber.Channel;
                string residenciaExchange = $"residencia.{residenciaId}";
                string comodoExchange = $"comodo.{comodoId}";
                channel.ExchangeDeclare(residenciaExchange, ExchangeType.Fanout, durable: true);
                channel.ExchangeDeclare(comodoExchange, ExchangeType.Fanout, durable: true);
                channel.ExchangeBind(destination: comodoExchange, source: residenciaExchange, routingKey: "");
            }
        }
    }
}

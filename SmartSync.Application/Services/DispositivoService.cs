using RabbitMQ.Client;
using SmartSync.Application.Interfaces;
using SmartSync.Domain.Entities;
using SmartSync.Infraestructure.Messaging.Interfaces;
using SmartSync.Infraestructure.Persistence.Interfaces;
using SmartSync.Infraestructure.Persistence.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSync.Application.Services
{
    public class DispositivoService(
        IDispositivoRepository repository,
        IComodoRepository comodoRepository,
        IRabbitMqPublisher publisher,
        IRabbitMqSubscriber subscriber
    ) : BaseService<Dispositivo>(repository, publisher), IDispositivoService
    {
        readonly IRabbitMqSubscriber _subscriber = subscriber;

        public override async Task<int> Insert(Dispositivo dispositivo)
        {
            var result = await base.Insert(dispositivo);

            // Busca o cômodo para obter o ResidenciaId correto
            var comodo = comodoRepository.Get(dispositivo.ComodoId).FirstOrDefault();
            if (comodo == null)
                throw new Exception("Cômodo não encontrado para o dispositivo.");

            string residenciaExchange = $"residencia.{comodo.ResidenciaId}";
            string comodoExchange = $"comodo.{dispositivo.ComodoId}";
            string queue = $"dispositivo.{dispositivo.Id}";

            if (_subscriber is SmartSync.Infraestructure.Messaging.Subscriber.RabbitMqSubscriber rabbitSubscriber)
            {
                var channel = rabbitSubscriber.Channel;
                // Garante que as exchanges existem
                channel.ExchangeDeclare(residenciaExchange, ExchangeType.Fanout, durable: true);
                channel.ExchangeDeclare(comodoExchange, ExchangeType.Fanout, durable: true);
                // Bind do cômodo para residência
                channel.ExchangeBind(destination: comodoExchange, source: residenciaExchange, routingKey: "");
                // Bind da fila do dispositivo para o exchange do cômodo
                channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
                channel.QueueBind(queue, comodoExchange, routingKey: "");
            }

            return result;
        }

        public override async Task<int> Update(Dispositivo dispositivo)
        {
            var dispositivoAtual = _repostitory.Get(dispositivo.Id).FirstOrDefault();
            if (dispositivoAtual == null)
                throw new Exception("Dispositivo não encontrado para update.");

            if (dispositivoAtual.ComodoId != dispositivo.ComodoId)
            {
                if (_subscriber is SmartSync.Infraestructure.Messaging.Subscriber.RabbitMqSubscriber rabbitSubscriber)
                {
                    var channel = rabbitSubscriber.Channel;
                    string oldComodoExchange = $"comodo.{dispositivoAtual.ComodoId}";
                    string newComodoExchange = $"comodo.{dispositivo.ComodoId}";
                    string queue = $"dispositivo.{dispositivo.Id}";

                    channel.QueueUnbind(queue, oldComodoExchange, routingKey: "");
                    channel.ExchangeDeclare(newComodoExchange, ExchangeType.Fanout, durable: true);
                    channel.QueueBind(queue, newComodoExchange, routingKey: "");
                }
            }

            return await base.Update(dispositivo);
        }

        public override async Task<int> Delete(Guid id)
        {
            // Opcional: Remover a fila do dispositivo (o subscriber pode recriar)
            if (_subscriber is SmartSync.Infraestructure.Messaging.Subscriber.RabbitMqSubscriber rabbitSubscriber)
            {
                var channel = rabbitSubscriber.Channel;
                string queue = $"dispositivo.{id}";
                channel.QueueDelete(queue, ifUnused: false, ifEmpty: false);
            }
            return await base.Delete(id);
        }
    }
}

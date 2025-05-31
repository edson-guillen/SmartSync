using Microsoft.Extensions.Hosting;
using SmartSync.Infraestructure.Messaging;
using SmartSync.Domain.Events;
using SmartSync.Domain.Entities;
using SmartSync.Infraestructure.Messaging.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Messaging.Subscriber
{
    public class EntityCreatedSubscriber : BackgroundService
    {
        private readonly IRabbitMqSubscriber _subscriber;

        public EntityCreatedSubscriber(IRabbitMqSubscriber subscriber)
        {
            _subscriber = subscriber;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _subscriber.Subscribe<EntityCreatedEvent<BaseModel>>("entity.created", async (evt) =>
            {
                Console.WriteLine($"Entidade criada: {evt.Entity.Id}");

                // Simula falha proposital para testar retry
                // Remova o throw depois que testar!
                throw new Exception("Erro forçado pra testar retry");
            });

            _subscriber.Start();

            return Task.CompletedTask;
        }
    }
}

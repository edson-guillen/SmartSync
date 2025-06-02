using Microsoft.Extensions.Hosting;
using SmartSync.Infraestructure.Messaging;
using SmartSync.Domain.Events;
using SmartSync.Domain.Entities;
using SmartSync.Infraestructure.Messaging.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using SmartSync.Infraestructure.Persistence.Interfaces;

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
            _subscriber.Subscribe<EntityCreatedEvent<BaseModel>>("smartsync", async (evt) =>
            {
                if (evt?.Entity != null)
                    Console.WriteLine($"Entidade criada: {evt.Entity.Id}");
                else
                    Console.WriteLine("Mensagem recebida sem entidade válida.");
            });

            _subscriber.Start();

            return Task.CompletedTask;
        }
    }
}
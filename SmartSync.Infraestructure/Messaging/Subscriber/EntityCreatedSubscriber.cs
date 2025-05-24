using Microsoft.Extensions.Hosting;
using SmartSync.Infraestructure.Messaging;
using SmartSync.Domain.Events;
using SmartSync.Domain.Entities;
using SmartSync.Infraestructure.Messaging.Interfaces;

namespace SmartSync.Infraestructure.Messaging.Subscriber
{
    public class EntityCreatedSubscriber(IRabbitMqSubscriber subscriber) : BackgroundService
    {
        private readonly IRabbitMqSubscriber _subscriber = subscriber;

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _subscriber.Subscribe<EntityCreatedEvent<BaseModel>>("entity.created", async (evt) =>
            {
                Console.WriteLine($"Entidade criada: {evt.Entity.Id}");
            });
            _subscriber.Start();
            return Task.CompletedTask;
        }
    }
}

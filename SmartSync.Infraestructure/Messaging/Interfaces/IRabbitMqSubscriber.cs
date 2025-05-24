using System;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Messaging.Interfaces
{
    public interface IRabbitMqSubscriber : IDisposable
    {
        void Subscribe<T>(string queue, Func<T, Task> onMessage);
        void Start();
        void Stop();
    }
}
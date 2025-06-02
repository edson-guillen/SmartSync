using System;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Messaging.Interfaces
{
    public interface IRabbitMqSubscriber : IDisposable
    {
        void Subscribe<T>(string queue, Func<T, Task> onMessage, int retryDelayMs = 10000, int maxRetries = 3);
        void SubscribeToFanout<T>(string exchange, string queue, Func<T, Task> onMessage);
        void Start(int maxRetries = 3);
        void Stop();
        void Dispose();
    }
}
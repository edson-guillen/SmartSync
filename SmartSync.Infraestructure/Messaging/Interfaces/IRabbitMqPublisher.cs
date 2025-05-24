using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Messaging.Interfaces
{
    public interface IRabbitMqPublisher : IDisposable
    {
        Task PublishAsync<T>(string queue, T message);
    }
}
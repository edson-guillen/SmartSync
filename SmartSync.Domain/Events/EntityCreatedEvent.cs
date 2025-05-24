using SmartSync.Domain.Entities;

namespace SmartSync.Domain.Events
{
    public class EntityCreatedEvent<T> : IEvent where T : BaseModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public T Entity { get; set; }
        public EntityCreatedEvent(T entity) => Entity = entity;
    }
}   
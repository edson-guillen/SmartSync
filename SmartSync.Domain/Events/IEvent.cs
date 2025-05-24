namespace SmartSync.Domain.Events
{
    public interface IEvent
    {
        Guid Id { get; set; }
        DateTime OccurredAt { get; set; }
    }
}
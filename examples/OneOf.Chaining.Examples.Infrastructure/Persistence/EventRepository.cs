using OneOf.Chaining.Examples.Domain.EventSourcing;

namespace OneOf.Chaining.Examples.Infrastructure.Persistence;

public class EventRepository : IEventRepository
{
    public List<PersistedEvent> PersistedEvents { get; } = new();

    public Task<PersistedEvent> InsertEvent(Guid groupingId, string eventClassName, string serialisedEvent)
    {
        var newPersistedEvent = new PersistedEvent(PersistedEvents.Count, groupingId, eventClassName, serialisedEvent, DateTime.UtcNow);
        PersistedEvents.Add(newPersistedEvent);
        return Task.FromResult(newPersistedEvent);
    }

    public Task<List<PersistedEvent>> InsertEvents(IList<(Guid groupingId, string eventClassName, string serialisedEvent)> events)
    {
        var newPersistedEvents = new List<PersistedEvent>();
        foreach (var (groupingId, eventClassName, serialisedEvent) in events)
        {
            var newPersistedEvent = new PersistedEvent(PersistedEvents.Count, groupingId, eventClassName, serialisedEvent, DateTime.UtcNow);
            PersistedEvents.Add(newPersistedEvent);
            newPersistedEvents.Add(newPersistedEvent);
        }
        
        return Task.FromResult(newPersistedEvents);
    }

    public Task<IEnumerable<PersistedEvent>> FetchEvents(Guid requestId)
    {
        return Task.FromResult(PersistedEvents.Where(pe => pe.RequestId == requestId));
    }
}


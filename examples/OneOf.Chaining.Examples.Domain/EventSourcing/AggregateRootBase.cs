using OneOf.Chaining.Examples.Domain.ServiceDefinitions;

namespace OneOf.Chaining.Examples.Domain.EventSourcing;

public class AggregateRootBase
{
    public Guid RequestId { get; }
    protected List<PersistedEvent> PersistedEvents { get; }
    protected IEventPersistenceService EventPersistenceService { get; }

    public AggregateRootBase(Guid requestId, List<PersistedEvent> persistedEvents, IEventPersistenceService eventPersistenceService)
    {
        RequestId = requestId;
        PersistedEvents = persistedEvents;
        EventPersistenceService = eventPersistenceService;
    }

    public async Task AppendEvent<T>(T eventAsT) where T : IDomainEvent
    {
        var @event = EventSourcing.Event.Create(eventAsT, RequestId);
        var persistedEvent = await EventPersistenceService.PersistEvent(@event);
        PersistedEvents.Add(persistedEvent);
    }

    public async Task AppendEvents(IEnumerable<Event> events)
    {
        if (events.Any(e => e.RequestId != this.RequestId))
            throw new Exception("All events must have the correct CrossBorderPaymentId");

        var persistedEvents = await EventPersistenceService.PersistEvents(events);
        persistedEvents.AddRange(persistedEvents);
    }

    public bool EventHasHappened<T>()
    {
        var eventClassName = typeof(T).FullName;
        return PersistedEvents.Any(pe => pe.EventClassName == eventClassName);
    }

    public T? Event<T>()
    {
        return PersistedEvents.To<T>();
    }

    public PersistedEvent? PersistedEvent<T>()
    {
        var eventClassName = typeof(T).FullName;
        return PersistedEvents.LastOrDefault(pe => pe.EventClassName == eventClassName);
    }
}
using Microsoft.Extensions.Logging;
using OneOf.Chaining.Examples.Domain.EventSourcing;
using OneOf.Chaining.Examples.Domain.ServiceDefinitions;

namespace OneOf.Chaining.Examples.Infrastructure.Persistence;

public class EventPersistenceService : IEventPersistenceService
{
    private readonly ILogger<EventPersistenceService> logger;
    private readonly IEventRepository eventRepository;

    public EventPersistenceService(
        ILogger<EventPersistenceService> logger,
        IEventRepository eventRepository)
    {
        this.logger = logger;
        this.eventRepository = eventRepository;
    }

    public async Task<PersistedEvent> PersistEvent(Event @event)
    {
        var persistedEvent = await eventRepository.InsertEvent(@event.RequestId, @event.EventClassName, @event.SerialisedEvent);
        return persistedEvent;
    }

    public async Task<List<PersistedEvent>> PersistEvents(IEnumerable<Event> events)
    {
        var persistedEvents = await eventRepository.InsertEvents(events.Select(e => (e.RequestId, e.EventClassName, SerialisedData: e.SerialisedEvent)).ToList());
        return persistedEvents.ToList();
    }

    public async Task<IEnumerable<PersistedEvent>> FetchEvents(Guid requestId)
    {
        var persistedEvents = await eventRepository.FetchEvents(requestId);
        return persistedEvents;
    }
}
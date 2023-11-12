using OneOf.Chaining.Examples.Domain.EventSourcing;

namespace OneOf.Chaining.Examples.Domain.ServiceDefinitions;
public interface IEventPersistenceService
{
    Task<PersistedEvent> PersistEvent(Event @event);
    Task<List<PersistedEvent>> PersistEvents(IEnumerable<Event> events);

    Task<IEnumerable<PersistedEvent>> FetchEvents(Guid requestId);

    //Task<PersistedEvent> AtomicallyPersistCrossBorderPaymentCompletedEventAndCreateOutboxRecord(Guid requestId, Guid locationId);
    //Task<PersistedEvent> AtomicallyPersistCrossBorderPaymentFailedEventAndCreateOutboxRecord(Guid requestId, Guid locationId, string failureReason);
}

using OneOf.Chaining.Examples.Domain.BusinessRules;
using OneOf.Chaining.Examples.Domain.DomainEvents;
using OneOf.Chaining.Examples.Domain.EventSourcing;
using OneOf.Chaining.Examples.Domain.Exceptions;
using OneOf.Chaining.Examples.Domain.Outcomes;
using OneOf.Chaining.Examples.Domain.ServiceDefinitions;

namespace OneOf.Chaining.Examples.Domain.Entities;

public class WeatherDataCollection : AggregateRootBase
{
    public WeatherDataCollection(Guid requestId, List<PersistedEvent> persistedEvents, IEventPersistenceService eventPersistenceService)
        : base(requestId, persistedEvents, eventPersistenceService)
    {
        Check.NotNull(PersistedEvents);

        var initiatedEvent = PersistedEvents.To<WeatherDataCollectionInitiated>();
        Check.NotNull(initiatedEvent);

        Check.NotNull(initiatedEvent.Data);
        Check.NotNull(initiatedEvent.Location);
    }

    // Properties from events guaranteed to be present because they come from the initiated event...
    public CollectedWeatherData Data => PersistedEvents.To<WeatherDataCollectionInitiated>()!.Data;
    public string Location => PersistedEvents.To<WeatherDataCollectionInitiated>()!.Location;

    // Properties from events which may not yet have happened, null if not yet happened.
    public Guid? LocationId => PersistedEvents.To<LocationIdFound>()!.LocationId;
    public Guid? ModelingSubmissionId => PersistedEvents.To<SubmittedToModeling>()!.SubmissionId;
    
    //bool? ModelingDataRejected,
    //bool? ModelingDataAccepted,
    //bool? SubmissionCompleted,
    //bool? ModelUpdated
    
    public static async Task<OneOf<WeatherDataCollection, Failure>> Hydrate(IEventPersistenceService eventPersistenceService, Guid requestId)
    {
        var persistedEvents = (await eventPersistenceService.FetchEvents(requestId)).ToList();

        if (persistedEvents.Count == 0)
            throw new ExpectedEventsNotFoundException();

        return new WeatherDataCollection(requestId, persistedEvents, eventPersistenceService);
    }

    public static async Task<OneOf<WeatherDataCollection, Failure>> PersistOrHydrate(IEventPersistenceService eventPersistenceService, Guid requestId, Event initialEvent)
    {
        var existingPersistedEvents = (await eventPersistenceService.FetchEvents(requestId)).ToList();

        if (existingPersistedEvents.Count != 0)
            return new WeatherDataCollection(requestId, existingPersistedEvents, eventPersistenceService);
        
        var initialEvents = new List<Event>
        {
            initialEvent,
            //EventSourcing.Event.Create(new LogicalExecutionContextPersisted(LogicalExecutionContext.From(logicalExecutionContextSnapshot)), crossBorderPaymentId, debtorInstitutionId)
        };
        var persistedInitialEvents = await eventPersistenceService.PersistEvents(initialEvents);

        var payment = new WeatherDataCollection(requestId, persistedInitialEvents, eventPersistenceService);
        return payment;
    }

    public async Task<OneOf<WeatherDataCollection, Failure>> AppendSubmissionCompleteEvent()
    {
        await AppendEvent(new SubmissionComplete());
        return this;
    }

    public async Task<OneOf<WeatherDataCollection, Failure>> AppendModelingDataAcceptedEvent()
    {
        await AppendEvent(new ModelingDataAccepted());
        return this;
    }

    public async Task<OneOf<WeatherDataCollection, Failure>> AppendModelingDataRejectedEvent(string reason)
    {
        await AppendEvent(new ModelingDataRejected(reason));
        return this;
    }

    public async Task<OneOf<WeatherDataCollection, Failure>> AppendModelUpdatedEvent()
    {
        await AppendEvent(new ModelUpdated());
        return this;
    }
}
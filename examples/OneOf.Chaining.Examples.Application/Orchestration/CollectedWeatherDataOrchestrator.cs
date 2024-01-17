using OneOf.Chaining.Examples.Application.Models.IntegrationEvents.WeatherModelingEvents;
using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain.Outcomes;
using OneOf.Chaining.Examples.Application.Models.Requests;
using OneOf.Chaining.Examples.Domain.DomainEvents;
using OneOf.Chaining.Examples.Domain.Entities;
using OneOf.Chaining.Examples.Domain.EventSourcing;
using OneOf.Chaining.Examples.Domain.ServiceDefinitions;

namespace OneOf.Chaining.Examples.Application.Orchestration;

public class CollectedWeatherDataOrchestrator : 
    IPostWeatherReportDataHandler,
    IEventHandler<ModelingDataAcceptedIntegrationEvent>, 
    IEventHandler<ModelingDataRejectedIntegrationEvent>,
    IEventHandler<ModelUpdatedIntegrationEvent>
{
    private readonly IEventPersistenceService eventPersistenceService;
    private readonly IWeatherModelingService weatherModelingService;
    private readonly INotificationService notificationService;

    public CollectedWeatherDataOrchestrator(
        IEventPersistenceService eventPersistenceService,
        IWeatherModelingService weatherModelingService,
        INotificationService notificationService
        )//IContributorPaymentService contributorPaymentService)
    {
        this.eventPersistenceService = eventPersistenceService;
        this.weatherModelingService = weatherModelingService;
        this.notificationService = notificationService;
    }

    public Task<OneOf<WeatherDataCollectionResponse, Failure>> Handle(string weatherDataLocation, CollectedWeatherDataModel weatherDataModel, 
        IWeatherDataValidator weatherDataValidator, 
        ILocationManager locationManager)
    {
        if (weatherDataValidator.Validate(weatherDataModel, out var errors) == false)
            return Task.FromResult(OneOf<WeatherDataCollectionResponse, Failure>.FromT1(new InvalidRequestFailure(errors)));

        var requestId = Guid.NewGuid();
        return WeatherDataCollection.PersistOrHydrate(eventPersistenceService, requestId, Event.Create(new WeatherDataCollectionInitiated(weatherDataModel.ToEntity(), weatherDataLocation), requestId))
            .Then(locationManager.Locate)
            .Then(weatherModelingService.Submit) // Calls async external service 
            .ToResult(WeatherDataCollectionResponse.FromWeatherDataCollection);
    }
    /*
     
    




































     */
    public async Task HandleEvent(ModelingDataAcceptedIntegrationEvent dataAcceptedIntegrationEvent)
    {
        var result = await WeatherDataCollection.Hydrate(eventPersistenceService, dataAcceptedIntegrationEvent.RequestId)
            .Then(x => x.AppendModelingDataAcceptedEvent())
            .Then(x => x.AppendSubmissionCompleteEvent());

        if (result.IsT1)
            throw new Exception($"Something went wrong while handling {nameof(ModelingDataAcceptedIntegrationEvent)}");
    }

    public async Task HandleEvent(ModelingDataRejectedIntegrationEvent dataRejectedIntegrationEvent)
    {
        var result = await WeatherDataCollection.Hydrate(eventPersistenceService, dataRejectedIntegrationEvent.RequestId)
            .Then(x => x.AppendModelingDataRejectedEvent(dataRejectedIntegrationEvent.Reason));

        if (result.IsT1)
            throw new Exception($"Something went wrong while handling {nameof(ModelingDataRejectedIntegrationEvent)}");
    }

    public async Task HandleEvent(ModelUpdatedIntegrationEvent integrationEvent)
    {
        var result = await WeatherDataCollection.Hydrate(eventPersistenceService, integrationEvent.RequestId)
            .Then(x => x.AppendModelUpdatedEvent())
            .Then(notificationService.NotifyModelUpdated);

        if (result.IsT1)
            throw new Exception($"Something went wrong while handling {nameof(ModelUpdatedIntegrationEvent)}");
    }
}

/*
* 1. roughly explain the new flow
* 2. add in the event handlers
* 3. explain WeatherDataCollection and domain events
* 4. Add in IContributorPaymentService
* 5. Show e2e component tests
*/
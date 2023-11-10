using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain.Outcomes;
using OneOf.Types;
using OneOf.Chaining.Examples.Application.Models.Requests;
using OneOf.Chaining.Examples.Application.Models.Events.WeatherModelingEvents;

namespace OneOf.Chaining.Examples.Application.Orchestration;

public class CollectedWeatherDataOrchestrator : 
    IPostWeatherReportDataHandler,
    IEventHandler<ModelingDataAcceptedEvent>, 
    IEventHandler<ModelingDataRejectedEvent>,
    IEventHandler<ModelUpdatedEvent>
{
    private readonly IWeatherDataPersistence weatherDataStore;
    private readonly IWeatherModelingService weatherModelingService;
    private readonly INotificationService notificationService;

    public CollectedWeatherDataOrchestrator(
        IWeatherDataPersistence weatherDataStore, 
        IWeatherModelingService weatherModelingService,
        INotificationService notificationService) 
        //IContributorPaymentService contributorPaymentService)
    {
        this.weatherDataStore = weatherDataStore;
        this.weatherModelingService = weatherModelingService;
        this.notificationService = notificationService;
    }

    public Task<OneOf<WeatherDataCollectionResponse, Failure>> Handle(string weatherDataLocation, CollectedWeatherDataModel weatherDataModel, 
        IWeatherDataValidator weatherDataValidator, 
        ILocationManager locationManager)
    {
        return CollectedWeatherDataDetails.FromRequest(weatherDataLocation, weatherDataModel)
            .Then(weatherDataValidator.Validate)
            .Then(locationManager.Locate)
            .Then(weatherDataStore.InsertOrFetch)
            .Then(weatherModelingService.Submit) // Calls async external service 
            .Then(weatherDataStore.UpdateStatusSubmittedToModeling)
            .ToResult(details => details.ToResponse());
    }
    /*
     
    




































     */
    public async Task HandleEvent(ModelingDataAcceptedEvent @event)
    {
        var result = await CollectedWeatherDataDetails.FromModelingEvent(@event)
            .Then(weatherDataStore.Fetch)
            .Then(weatherDataStore.UpdateStatusAcceptedIntoModeling)
            .Then(weatherDataStore.CompleteSubmission);

        if (result.IsT1)
            throw new Exception($"Something went wrong while handling {nameof(ModelingDataAcceptedEvent)}");
    }

    public async Task HandleEvent(ModelingDataRejectedEvent @event)
    {
        var result = await CollectedWeatherDataDetails.FromModelingEvent(@event)
            .Then(weatherDataStore.Fetch)
            .Then(weatherDataStore.UpdateStatusDataRejected);

        if (result.IsT1)
            throw new Exception($"Something went wrong while handling {nameof(ModelingDataRejectedEvent)}");
    }

    public async Task HandleEvent(ModelUpdatedEvent @event)
    {
        var result = await CollectedWeatherDataDetails.FromModelingEvent(@event)
            .Then(weatherDataStore.Fetch)
            .Then(weatherDataStore.UpdateStatusModelUpdated)
            .Then(notificationService.NotifyModelUpdated);

        if (result.IsT1)
            throw new Exception($"Something went wrong while handling {nameof(ModelUpdatedEvent)}");
    }
}

/*
 * 1. roughly explain the new flow
 * 2. add in the event handlers
 * 3. shared details object + using records
 * 4. Add in IContributorPaymentService
 * 5. benefits of all flows being in one place - intent is the star of the show!
 */
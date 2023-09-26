using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain.Outcomes;
using OneOf.Types;
using OneOf.Chaining.Examples.Application.Models.Requests;
using OneOf.Chaining.Examples.Application.Models.Events.WeatherModelingEvents;

namespace OneOf.Chaining.Examples.Application.Orchestration;

public class CollectedWeatherDataOrchestrator : 
    IPostWeatherReportDataHandler,
    IEventHandler<DataAcceptedEvent>, 
    IEventHandler<DataRejectedEvent>,
    IEventHandler<ModelUpdatedEvent>
{
    private readonly IWeatherDataPersistence weatherDataStore;
    private readonly IWeatherModelingService weatherModelingService;
    private readonly IContributorPaymentService contributorPaymentService;
    private readonly INotificationService notificationService;

    public CollectedWeatherDataOrchestrator(
        IWeatherDataPersistence weatherDataStore, 
        IWeatherModelingService weatherModelingService,
        IContributorPaymentService contributorPaymentService,
        INotificationService notificationService)
    {
        this.weatherDataStore = weatherDataStore;
        this.weatherModelingService = weatherModelingService;
        this.contributorPaymentService = contributorPaymentService;
        this.notificationService = notificationService;
    }

    public Task<OneOf<Success, Failure>> Handle(string weatherDataLocation, CollectedWeatherDataModel weatherDataModel, 
        IWeatherDataValidator weatherDataValidator, 
        ILocationManager locationManager)
    {
        return CollectedWeatherDataDetails.FromRequest(weatherDataLocation, weatherDataModel)
            .Then(weatherDataValidator.Validate)
            .Then(locationManager.Locate)
            .Then(weatherDataStore.InsertOrFetch)
            .Then(contributorPaymentService.CreatePendingPayment)
            .Then(weatherModelingService.Submit, // Calls async external service
                onFailure: (details, _) => contributorPaymentService.RevokePendingPayment(details)) 
            .Then(weatherDataStore.UpdateStatusSubmittedToModeling)
            .ToResult();
    }
    /*
     
    




































     */
    public async Task HandleEvent(DataAcceptedEvent @event)
    {
        var result = await CollectedWeatherDataDetails.FromModelingEvent(@event)
            .Then(weatherDataStore.Fetch)
            .Then(weatherDataStore.UpdateStatusAcceptedIntoModeling)
            .Then(contributorPaymentService.CommitPendingPayment)
            .Then(weatherDataStore.CompleteSubmission);

        if (result.IsT1)
            throw new Exception($"Something went wrong while handling {nameof(DataAcceptedEvent)}");
    }

    public async Task HandleEvent(DataRejectedEvent @event)
    {
        var result = await CollectedWeatherDataDetails.FromModelingEvent(@event)
            .Then(weatherDataStore.Fetch)
            .Then(weatherDataStore.UpdateStatusDataRejected)
            .Then(contributorPaymentService.RevokePendingPayment);

        if (result.IsT1)
            throw new Exception($"Something went wrong while handling {nameof(DataRejectedEvent)}");
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
 * 4. benefits of all flows being in one place
 */


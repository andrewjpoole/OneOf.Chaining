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
    private readonly IWeatherDataPersistence _weatherDataStore;
    private readonly WeatherModelingService _weatherModelingService;
    private readonly IContributorPaymentService _contributorPaymentService;
    private readonly INotificationService _notificationService;

    public CollectedWeatherDataOrchestrator(
        IWeatherDataPersistence weatherDataStore, 
        WeatherModelingService weatherModelingService,
        IContributorPaymentService contributorPaymentService,
        INotificationService notificationService)
    {
        _weatherDataStore = weatherDataStore;
        _weatherModelingService = weatherModelingService;
        _contributorPaymentService = contributorPaymentService;
        _notificationService = notificationService;
    }

    public Task<OneOf<Success, Failure>> Handle(string weatherDataLocation, CollectedWeatherDataModel weatherDataModel, 
        IWeatherDataValidator weatherDataValidator, 
        ILocationManager locationManager)
    {
        return CollectedWeatherDataDetails.FromRequest(weatherDataLocation, weatherDataModel)
            .Then(weatherDataValidator.Validate)
            .Then(locationManager.Locate)
            .Then(_weatherDataStore.InsertOrFetch)
            .Then(_contributorPaymentService.CreatePendingPayment)
            .Then(_weatherModelingService.Submit, // Calls async external service
                onFailure: (details, _) => _contributorPaymentService.RevokePendingPayment(details)) 
            .Then(_weatherDataStore.UpdateStatusSubmittedToModeling)
            .ToResult();
    }
    
    public async Task HandleEvent(DataAcceptedEvent @event)
    {
        var result = await CollectedWeatherDataDetails.FromModelingEvent(@event)
            .Then(_weatherDataStore.Fetch)
            .Then(_weatherDataStore.UpdateStatusModelingSucceeded)
            .Then(_contributorPaymentService.CommitPendingPayment)
            .Then(_weatherDataStore.CompleteSubmission);

        if (result.IsT1)
            throw new Exception($"Something went wrong while handling {nameof(DataAcceptedEvent)}");
    }

    public async Task HandleEvent(DataRejectedEvent @event)
    {
        var result = await CollectedWeatherDataDetails.FromModelingEvent(@event)
            .Then(_weatherDataStore.Fetch)
            .Then(_weatherDataStore.UpdateStatusDataRejected)
            .Then(_contributorPaymentService.RevokePendingPayment);

        if (result.IsT1)
            throw new Exception($"Something went wrong while handling {nameof(DataRejectedEvent)}");
    }

    public async Task HandleEvent(ModelUpdatedEvent @event)
    {
        var result = await CollectedWeatherDataDetails.FromModelingEvent(@event)
            .Then(_weatherDataStore.Fetch)
            .Then(_weatherDataStore.UpdateStatusModelUpdated)
            .Then(_notificationService.NotifyModelUpdated);

        if (result.IsT1)
            throw new Exception($"Something went wrong while handling {nameof(ModelUpdatedEvent)}");
    }
}
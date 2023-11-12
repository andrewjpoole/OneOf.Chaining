using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain.DomainEvents;
using OneOf.Chaining.Examples.Domain.Entities;
using OneOf.Chaining.Examples.Domain.Outcomes;
using OneOf.Chaining.Examples.Infrastructure.ApiClients.WeatherModelingSystem;

namespace OneOf.Chaining.Examples.Infrastructure.ModelingService;

public class WeatherModelingService : IWeatherModelingService
{
    private readonly IRefitClientWrapper<IWeatherModelingServiceClient> weatherModelingServiceClientWrapper;

    public WeatherModelingService(IRefitClientWrapper<IWeatherModelingServiceClient> weatherModelingServiceClientWrapper)
    {
        this.weatherModelingServiceClientWrapper = weatherModelingServiceClientWrapper;
    }

    public async Task<OneOf<WeatherDataCollection, Failure>> Submit(WeatherDataCollection weatherDataCollection)
    {
        // calls out to an external service which returns an Accepted response
        // the result will be communicated via a service bus message...

        using var weatherModelingServiceClient = weatherModelingServiceClientWrapper.CreateClient();

        var response = await weatherModelingServiceClient.PostCollectedData(weatherDataCollection.Location, weatherDataCollection); // response is null in LoggingHttpMessageHandler.Log.RequestEnd ???
        var bodyContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode) 
            return OneOf<WeatherDataCollection, Failure>.FromT1(new WeatherModelingServiceRejectionFailure(bodyContent));
        
        var submissionId = Guid.Parse(bodyContent);
        await weatherDataCollection.AppendEvent(new SubmittedToModeling(submissionId));

        return weatherDataCollection;
    }
}
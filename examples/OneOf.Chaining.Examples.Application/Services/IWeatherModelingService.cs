using OneOf.Chaining.Examples.Application.Orchestration;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Services;

public class WeatherModelingService : IWeatherModelingService
{
    public async Task<OneOf<CollectedWeatherDataDetails, Failure>> Submit(CollectedWeatherDataDetails details)
    {
        // calls out to an external service which returns an Accepted response
        // the result will be communicated via a service bus message...

        await Task.Delay(100);

        return details;
    }
}

public interface IWeatherModelingService
{
    Task<OneOf<CollectedWeatherDataDetails, Failure>> Submit(CollectedWeatherDataDetails details);
}
using OneOf.Chaining.Examples.Application.Orchestration;
using Refit;

namespace OneOf.Chaining.Examples.Infrastructure.ApiClients.WeatherModelingSystem;

public interface IWeatherModelingServiceClient : IDisposable
{
    [Post("/v1/collected-weather-data/{location}")]
    Task<HttpResponseMessage> PostCollectedData(string location, [Body] CollectedWeatherDataDetails collectedWeatherData);
}
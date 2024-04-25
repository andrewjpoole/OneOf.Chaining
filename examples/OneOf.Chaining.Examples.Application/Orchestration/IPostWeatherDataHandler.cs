using OneOf.Chaining.Examples.Application.Models.Requests;
using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Orchestration;

public interface IPostWeatherDataHandler
{
    Task<OneOf<WeatherDataCollectionResponse, Failure>> HandlePostWeatherData(string weatherDataLocation, CollectedWeatherDataModel weatherDataModel, 
        IWeatherDataValidator weatherDataValidator, ILocationManager locationManager);
}
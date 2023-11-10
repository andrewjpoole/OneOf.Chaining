using OneOf.Chaining.Examples.Application.Models.Requests;
using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Orchestration;

public interface IPostWeatherReportDataHandler
{
    Task<OneOf<WeatherDataCollectionResponse, Failure>> Handle(string weatherDataLocation, CollectedWeatherDataModel weatherDataModel, 
        IWeatherDataValidator weatherDataValidator, ILocationManager locationManager);
}
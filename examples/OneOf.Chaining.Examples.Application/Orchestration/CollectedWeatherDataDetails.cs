using OneOf.Chaining.Examples.Application.Models.Events.WeatherModelingEvents;
using OneOf.Chaining.Examples.Application.Models.Requests;
using OneOf.Chaining.Examples.Domain.Entities;
using OneOf.Chaining.Examples.Domain.Outcomes;
using OneOf.Chaining.Examples.Domain.ValueObjects;

namespace OneOf.Chaining.Examples.Application.Orchestration;

public record CollectedWeatherDataDetails(
    Guid RequestId,
    string? Location, 
    CollectedWeatherData? Data,
    Guid? LocationId = null)
{
    public static Task<OneOf<CollectedWeatherDataDetails, Failure>> FromRequest(string location, CollectedWeatherDataModel data)
    {
        var points = new List<CollectedWeatherDataPoint>();
        foreach (var collectedReading in data.Points)
        {
            points.Add(new CollectedWeatherDataPoint(
                Guid.NewGuid(), 
                collectedReading.time, 
                new WindSpeed(collectedReading.WindSpeedsInMetersPerSecond), 
                new WindDirection(collectedReading.WindDirection), 
                new Temperature(collectedReading.TemperatureReadingsInDegreesCelcius),
                new Humidity(collectedReading.HumidityReadingsInPercent)));
        }
        var collectedDataEntity = new CollectedWeatherData(points);

        return Task.FromResult(OneOf<CollectedWeatherDataDetails, Failure>.FromT0(new CollectedWeatherDataDetails(Guid.NewGuid(), location, collectedDataEntity)));
    }

    public static Task<OneOf<CollectedWeatherDataDetails, Failure>> FromModelingEvent(ModelingEvent @event)
    {
        return Task.FromResult(OneOf<CollectedWeatherDataDetails, Failure>.FromT0(new CollectedWeatherDataDetails(@event.RequestId, null, null, null)));
    }
}
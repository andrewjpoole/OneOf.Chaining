using OneOf.Chaining.Examples.Application.Models;
using OneOf.Chaining.Examples.Application.Models.Events.WeatherModelingEvents;
using OneOf.Chaining.Examples.Application.Models.Requests;
using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain.Entities;
using OneOf.Chaining.Examples.Domain.Outcomes;
using OneOf.Chaining.Examples.Domain.ValueObjects;
using OneOf.Types;

namespace OneOf.Chaining.Examples.Application.Orchestration;

public record CollectedWeatherDataDetails(
    Guid RequestId,
    string? Location, 
    CollectedWeatherData? Data,
    Guid? LocationId,
    bool? SubmittedToModeling,
    Guid? WeatherModelingServiceSubmissionId,
    bool? ModelingDataRejected,
    bool? ModelingDataAccepted,
    bool? SubmissionCompleted,
    bool? ModelUpdated)
{
    public static Task<OneOf<CollectedWeatherDataDetails, Failure>> FromRequest(string location, CollectedWeatherDataModel data)
    {
        var points = new List<CollectedWeatherDataPoint>();
        foreach (var collectedReading in data.Points)
        {
            points.Add(new CollectedWeatherDataPoint(
                Guid.NewGuid(), 
                collectedReading.time, 
                new WindSpeed(collectedReading.WindSpeedInMetersPerSecond), 
                new WindDirection(collectedReading.WindDirection), 
                new Temperature(collectedReading.TemperatureReadingInDegreesCelcius),
                new Humidity(collectedReading.HumidityReadingInPercent)));
        }
        var collectedDataEntity = new CollectedWeatherData(points);

        return Task.FromResult(OneOf<CollectedWeatherDataDetails, Failure>.FromT0(
            new CollectedWeatherDataDetails(
                Guid.NewGuid(), 
                location, 
                collectedDataEntity, 
                null, 
                null, 
                null, 
                null,
                null, 
                null, 
                null)));
    }

    public static Task<OneOf<CollectedWeatherDataDetails, Failure>> FromModelingEvent(ModelingEvent @event)
    {
        return Task.FromResult(OneOf<CollectedWeatherDataDetails, Failure>.FromT0(
            new CollectedWeatherDataDetails(
                Guid.Empty, 
                null, 
                null, 
                null, 
                null, 
                @event.ModelingSubmissionId, 
                null,
                null, 
                null, 
                null)));
    }

    public CollectedWeatherDataDetails WithUpdatedState(IEnumerable<CollectedWeatherDataDetailsStatusUpdate> statusEvents)
    {
        var statusEventList = statusEvents.OrderBy(s => s.TimeStamp).ToList();

        var submittedToModeling = statusEventList.LastOrDefault(r => r.EventName == EventNames.SubmittedToModeling.ToString())
            is not null;

        var modelingDataRejected = statusEventList.LastOrDefault(r => r.EventName == EventNames.ModelingDataRejected.ToString())
            is not null;

        var modelingDataAccepted = statusEventList.LastOrDefault(r => r.EventName == EventNames.ModelingDataAccepted.ToString())
            is not null;

        var modelUpdated = statusEventList.LastOrDefault(r => r.EventName == EventNames.ModelUpdated.ToString())
            is not null;

        return this with
        {
            SubmittedToModeling = submittedToModeling,
            ModelingDataRejected = modelingDataRejected,
            ModelingDataAccepted = modelingDataAccepted,
            ModelUpdated = modelUpdated
        };
    }

    public WeatherDataCollectionResponse ToResponse()
    {
        return new WeatherDataCollectionResponse(RequestId);
    }
}


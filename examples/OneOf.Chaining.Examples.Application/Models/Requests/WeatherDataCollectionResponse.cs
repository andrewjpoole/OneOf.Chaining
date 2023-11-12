using OneOf.Chaining.Examples.Domain.Entities;

namespace OneOf.Chaining.Examples.Application.Models.Requests;

public record WeatherDataCollectionResponse(Guid RequestId)
{
    public static WeatherDataCollectionResponse FromWeatherDataCollection(WeatherDataCollection weatherDataCollection)
    {
        return new WeatherDataCollectionResponse(weatherDataCollection.RequestId);
    }
}
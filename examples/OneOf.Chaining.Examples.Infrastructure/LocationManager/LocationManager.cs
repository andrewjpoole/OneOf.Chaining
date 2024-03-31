using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain.DomainEvents;
using OneOf.Chaining.Examples.Domain.Entities;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Infrastructure.LocationManager;

public class LocationManager : ILocationManager
{
    private readonly Dictionary<string, Guid> knownLocations = [];

    public async Task<OneOf<WeatherDataCollection, Failure>> Locate(WeatherDataCollection weatherDataCollection)
    {
        if (!knownLocations.ContainsKey(weatherDataCollection.Location))
            knownLocations.Add(weatherDataCollection.Location, Guid.NewGuid());
        
        await weatherDataCollection.AppendEvent(new LocationIdFound(knownLocations[weatherDataCollection.Location]));

        return weatherDataCollection;
    }
}
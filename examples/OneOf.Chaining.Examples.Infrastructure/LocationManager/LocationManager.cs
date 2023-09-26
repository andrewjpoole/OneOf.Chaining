using OneOf.Chaining.Examples.Application.Orchestration;
using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Infrastructure.LocationManager;

public class LocationManager : ILocationManager
{
    private readonly Dictionary<string, Guid> knownLocations = new();

    public async Task<OneOf<CollectedWeatherDataDetails, Failure>> Locate(CollectedWeatherDataDetails details)
    {
        if (!knownLocations.ContainsKey(details.Location))
            knownLocations.Add(details.Location, Guid.NewGuid());

        var augmentedDetails = details with { LocationId = knownLocations[details.Location] };
        
        return augmentedDetails;

        //return OneOf<WeatherReport, Failure>.FromT1(new InvalidWeatherDataFailure(details.Location));
    }
}
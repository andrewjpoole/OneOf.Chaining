using OneOf.Chaining.Examples.Application.Orchestration;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Services;

public class WeatherDataValidator : IWeatherDataValidator
{
    public async Task<OneOf<CollectedWeatherDataDetails, Failure>> Validate(CollectedWeatherDataDetails details)
    {

        if (true)
            return details;

        //return OneOf<WeatherReport, Failure>.FromT1(new InvalidWeatherDataFailure(details.Location));
    }
}

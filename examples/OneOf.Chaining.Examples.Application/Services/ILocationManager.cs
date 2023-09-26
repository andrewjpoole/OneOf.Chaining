using OneOf.Chaining.Examples.Application.Orchestration;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Services;

public interface ILocationManager
{
    Task<OneOf<CollectedWeatherDataDetails, Failure>> Locate(CollectedWeatherDataDetails details);
}
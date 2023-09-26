using OneOf.Chaining.Examples.Application.Orchestration;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Services;

public interface IWeatherDataValidator
{
    Task<OneOf<CollectedWeatherDataDetails, Failure>> Validate(CollectedWeatherDataDetails details);
}
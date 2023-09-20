using OneOf.Chaining.Examples.Application.Orchestration;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Services;

public interface INotificationService
{
    Task<OneOf<CollectedWeatherDataDetails, Failure>> NotifyModelUpdated(CollectedWeatherDataDetails details);
}
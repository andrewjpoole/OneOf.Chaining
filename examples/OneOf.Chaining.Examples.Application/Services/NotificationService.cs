using OneOf.Chaining.Examples.Application.Orchestration;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Services;

public class NotificationService : INotificationService
{
    Task<OneOf<CollectedWeatherDataDetails, Failure>> INotificationService.NotifyModelUpdated(CollectedWeatherDataDetails details)
    {
        throw new NotImplementedException();
    }
}
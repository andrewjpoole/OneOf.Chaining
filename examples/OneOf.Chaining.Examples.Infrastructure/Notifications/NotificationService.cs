using OneOf.Chaining.Examples.Application.Orchestration;
using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Infrastructure.Notifications
{
    public class NotificationService : INotificationService
    {
        public Task<OneOf<CollectedWeatherDataDetails, Failure>> NotifyModelUpdated(CollectedWeatherDataDetails details)
        {
            throw new NotImplementedException();
        }
    }
}

using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain.Entities;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Infrastructure.Notifications;

public class NotificationService : INotificationService
{
    public Task<OneOf<WeatherDataCollection, Failure>> NotifyModelUpdated(WeatherDataCollection weatherDataCollection)
    {
        // todo: simulate some notification maybe?
        return Task.FromResult(OneOf<WeatherDataCollection, Failure>.FromT0(weatherDataCollection));
    }
}
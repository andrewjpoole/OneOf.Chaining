using OneOf.Chaining.Examples.Domain.Entities;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Services;

public interface INotificationService
{
    Task<OneOf<WeatherDataCollection, Failure>> NotifyModelUpdated(WeatherDataCollection weatherDataCollection);
}
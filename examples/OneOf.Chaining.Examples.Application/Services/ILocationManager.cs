using OneOf.Chaining.Examples.Domain.Entities;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Services;

public interface ILocationManager
{
    Task<OneOf<WeatherDataCollection, Failure>> Locate(WeatherDataCollection weatherDataCollection);
}
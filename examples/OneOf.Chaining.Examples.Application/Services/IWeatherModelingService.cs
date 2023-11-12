using OneOf.Chaining.Examples.Domain.Entities;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Services;

public interface IWeatherModelingService
{
    Task<OneOf<WeatherDataCollection, Failure>> Submit(WeatherDataCollection weatherDataCollection);
}
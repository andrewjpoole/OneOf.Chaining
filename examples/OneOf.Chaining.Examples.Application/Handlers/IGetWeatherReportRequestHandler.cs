using OneOf.Chaining.Examples.Domain;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Handlers;

public interface IGetWeatherReportRequestHandler
{
    Task<OneOf<WeatherReport, Failure>> Handle(string requestedRegion, DateTime requestedDate);
}

// OneOf gives us discriminated unions in C#!
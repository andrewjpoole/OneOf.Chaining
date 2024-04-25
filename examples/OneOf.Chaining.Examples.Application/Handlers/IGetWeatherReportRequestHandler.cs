using OneOf.Chaining.Examples.Application.Models;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Handlers;

public interface IGetWeatherReportRequestHandler
{
    Task<OneOf<WeatherReportResponse, Failure>> HandleGetWeatherReport(string requestedRegion, DateTime requestedDate);
}

// OneOf gives us discriminated unions in C#!
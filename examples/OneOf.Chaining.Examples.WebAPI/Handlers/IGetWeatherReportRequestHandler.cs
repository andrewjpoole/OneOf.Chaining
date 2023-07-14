namespace OneOf.Chaining.Examples.WebAPI.Handlers;

public interface IGetWeatherReportRequestHandler
{
    Task<OneOf<WeatherReport, Failure>> Handle(string requestedRegion, DateTime requestedDate);
}
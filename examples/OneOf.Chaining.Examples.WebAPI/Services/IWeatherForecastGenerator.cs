namespace OneOf.Chaining.Examples.WebAPI.Services;

public interface IWeatherForecastGenerator
{
    Task<OneOf<WeatherReport, Failure>> Generate(WeatherReport report);
}
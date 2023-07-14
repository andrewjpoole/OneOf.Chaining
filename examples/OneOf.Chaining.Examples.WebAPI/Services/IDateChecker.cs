namespace OneOf.Chaining.Examples.WebAPI.Services;

public interface IDateChecker
{
    Task<OneOf<WeatherReport, Failure>> CheckDate(WeatherReport report);
}
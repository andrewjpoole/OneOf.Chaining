namespace OneOf.Chaining.Examples.WebAPI.Services;

public interface IRegionValidator
{
    Task<OneOf<WeatherReport, Failure>> ValidateRegion(WeatherReport report);
}
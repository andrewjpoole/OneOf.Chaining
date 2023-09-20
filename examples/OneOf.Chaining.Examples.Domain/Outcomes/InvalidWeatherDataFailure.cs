namespace OneOf.Chaining.Examples.Domain.Outcomes;

public class InvalidWeatherDataFailure
{
    public string Title => "Invalid WeatherData";

    public string Detail { get; }

    public InvalidWeatherDataFailure(string detail)
    {
        Detail = detail;
    }
}
namespace OneOf.Chaining.Examples.WebAPI.Services;

public class WeatherForecastGenerator : IWeatherForecastGenerator
{
    private string[] summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public async Task<OneOf<WeatherReport, Failure>> Generate(WeatherReport report)
    {
        report.Set(summaries[Random.Shared.Next(summaries.Length)], Random.Shared.Next(-20, 55));
        return report;
    }
}
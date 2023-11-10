namespace OneOf.Chaining.Examples.Infrastructure.ApiClients.WeatherModelingSystem;

public class WeatherModelingServiceOptions
{
    public static string ConfigSectionName => "WeatherModelingServiceOptions";

    public string? BaseUrl { get; set; }
    public string? SubscriptionKey { get; set; }
    public int MaxRetryCount { get; set; } = 3;
    public string ApiManagerSubscriptionKeyHeader { get; set; } = "API-Key";
}
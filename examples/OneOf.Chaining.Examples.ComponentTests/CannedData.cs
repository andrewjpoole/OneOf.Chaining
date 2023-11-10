using OneOf.Chaining.Examples.Application.Models.Requests;
using OneOf.Chaining.Examples.Domain.Entities;
using OneOf.Chaining.Examples.Domain.ValueObjects;

namespace OneOf.Chaining.Examples.Tests;

public static class CannedData
{
    private static readonly Random Random = new();
    //public static WindSpeed GetRandomWindSpeed() => new((decimal)(Random.Next(1, 69) + Random.NextSingle()));
    //public static WindDirection GetRandomWindDirection() => new(new List<string>{"N", "NE", "E", "SE", "S", "SW", "W", "NW"}[Random.Next(0,7)]);
    //public static Temperature GetRandomTemperature() => new((decimal)(Random.Next(-15, 45) + Random.NextSingle()));
    //public static Humidity GetRandomHumidity() => new((decimal)Random.NextSingle());

    public static decimal GetRandomWindSpeed() => (decimal)(Random.Next(1, 69) + Random.NextSingle());
    public static string GetRandomWindDirection() => new List<string> { "N", "NE", "E", "SE", "S", "SW", "W", "NW" }[Random.Next(0, 7)];
    public static decimal GetRandomTemperature() => (decimal)(Random.Next(-15, 45) + Random.NextSingle());
    public static decimal GetRandomHumidity() => (decimal)Random.NextSingle();

    //public static CollectedWeatherDataPoint GetRandCollectedWeatherData() =>
    //    new (
    //        Guid.NewGuid(),
    //        DateTimeOffset.Now,
    //        GetRandomWindSpeed(),
    //        GetRandomWindDirection(),
    //        GetRandomTemperature(),
    //        GetRandomHumidity());
    
    public static CollectedWeatherDataPointModel GetRandCollectedWeatherDataModel() =>
        new (
            DateTimeOffset.UtcNow, 
            GetRandomWindSpeed(),
            GetRandomWindDirection(),
            GetRandomTemperature(),
            GetRandomHumidity());
}
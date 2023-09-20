using OneOf.Chaining.Examples.Domain.ValueObjects;

namespace OneOf.Chaining.Examples.Domain.Entities;

public record CollectedWeatherDataPoint(
    Guid Id,
    DateTimeOffset time,
    WindSpeed WindSpeed,
    WindDirection WindDirection,
    Temperature Temperature,
    Humidity Humidity
);
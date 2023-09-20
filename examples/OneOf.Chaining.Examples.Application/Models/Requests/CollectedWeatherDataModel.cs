namespace OneOf.Chaining.Examples.Application.Models.Requests;

public record CollectedWeatherDataPointModel(
    DateTimeOffset time,
    decimal WindSpeedsInMetersPerSecond,
    string WindDirection,
    decimal TemperatureReadingsInDegreesCelcius,
    decimal HumidityReadingsInPercent
    );

public record CollectedWeatherDataModel(List<CollectedWeatherDataPointModel> Points);
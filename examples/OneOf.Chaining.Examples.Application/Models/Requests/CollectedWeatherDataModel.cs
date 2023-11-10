namespace OneOf.Chaining.Examples.Application.Models.Requests;

public record CollectedWeatherDataPointModel(
    DateTimeOffset time,
    decimal WindSpeedInMetersPerSecond,
    string WindDirection,
    decimal TemperatureReadingInDegreesCelcius,
    decimal HumidityReadingInPercent
    );

public record CollectedWeatherDataModel(List<CollectedWeatherDataPointModel> Points);
namespace OneOf.Chaining.Examples.Application.Models;

public record CollectedWeatherDataDetailsStatusUpdate(Guid CollectedWeatherDetailsRequestId, DateTime TimeStamp, string EventName, string ExtraInfo = "");
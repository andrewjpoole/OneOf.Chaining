namespace OneOf.Chaining.Examples.Application.Models.Events.WeatherModelingEvents;

public record ModelingDataRejectedEvent(Guid RequestId, string Reason) : ModelingEvent(RequestId);
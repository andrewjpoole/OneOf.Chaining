namespace OneOf.Chaining.Examples.Application.Models.Events.WeatherModelingEvents;

public record ModelingDataAcceptedEvent(Guid RequestId) : ModelingEvent(RequestId);
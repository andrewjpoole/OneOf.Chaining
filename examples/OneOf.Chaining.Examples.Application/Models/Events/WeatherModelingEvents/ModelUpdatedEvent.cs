namespace OneOf.Chaining.Examples.Application.Models.Events.WeatherModelingEvents;

public record ModelUpdatedEvent(Guid RequestId) : ModelingEvent(RequestId);
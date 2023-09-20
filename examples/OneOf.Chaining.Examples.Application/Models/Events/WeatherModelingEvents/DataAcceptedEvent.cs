namespace OneOf.Chaining.Examples.Application.Models.Events.WeatherModelingEvents;

public record DataAcceptedEvent(Guid RequestId) : ModelingEvent(RequestId);
namespace OneOf.Chaining.Examples.Application.Models.IntegrationEvents.WeatherModelingEvents;

public record ModelUpdatedEvent(Guid RequestId) : ModelingEvent(RequestId);
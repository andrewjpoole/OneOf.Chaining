namespace OneOf.Chaining.Examples.Application.Models.IntegrationEvents.WeatherModelingEvents;

public record ModelUpdatedIntegrationEvent(Guid RequestId) : ModelingIntegrationEvent(RequestId);
namespace OneOf.Chaining.Examples.Application.Models.IntegrationEvents.WeatherModelingEvents;

public record ModelingDataAcceptedIntegrationEvent(Guid RequestId) : ModelingIntegrationEvent(RequestId);
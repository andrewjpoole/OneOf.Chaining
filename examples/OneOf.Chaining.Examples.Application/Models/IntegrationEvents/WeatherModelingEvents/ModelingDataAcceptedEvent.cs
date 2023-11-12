namespace OneOf.Chaining.Examples.Application.Models.IntegrationEvents.WeatherModelingEvents;

public record ModelingDataAcceptedEvent(Guid RequestId) : ModelingEvent(RequestId);
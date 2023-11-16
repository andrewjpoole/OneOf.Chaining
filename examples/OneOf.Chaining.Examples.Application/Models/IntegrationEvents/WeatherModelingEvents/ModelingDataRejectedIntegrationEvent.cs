namespace OneOf.Chaining.Examples.Application.Models.IntegrationEvents.WeatherModelingEvents;

public record ModelingDataRejectedIntegrationEvent(Guid RequestId, string Reason) : ModelingIntegrationEvent(RequestId);
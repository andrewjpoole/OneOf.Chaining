namespace OneOf.Chaining.Examples.Application.Models.IntegrationEvents.WeatherModelingEvents;

public record ModelingDataRejectedEvent(Guid RequestId, string Reason) : ModelingEvent(RequestId);
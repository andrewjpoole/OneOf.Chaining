namespace OneOf.Chaining.Examples.Application.Models.Events.WeatherModelingEvents;

public record DataRejectedEvent(Guid RequestId, string Reason) : ModelingEvent(RequestId);
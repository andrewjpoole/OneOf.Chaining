using OneOf.Chaining.Examples.Domain.Entities;
using OneOf.Chaining.Examples.Domain.EventSourcing;

namespace OneOf.Chaining.Examples.Domain.DomainEvents;

public record WeatherDataCollectionInitiated(CollectedWeatherData Data, string Location) : IDomainEvent;
public record LocationIdFound(Guid LocationId) : IDomainEvent;
public record SubmittedToModeling(Guid SubmissionId) : IDomainEvent;
public record ModelingDataAccepted() : IDomainEvent;
public record SubmissionComplete() : IDomainEvent;
public record ModelingDataRejected(string Reason) : IDomainEvent;
public record ModelUpdated() : IDomainEvent;
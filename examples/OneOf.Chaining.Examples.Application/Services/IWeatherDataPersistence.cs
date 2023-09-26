using OneOf.Chaining.Examples.Application.Orchestration;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Services;

public interface IWeatherDataPersistence
{
    Task<OneOf<CollectedWeatherDataDetails, Failure>> InsertOrFetch(CollectedWeatherDataDetails details);
    Task<OneOf<CollectedWeatherDataDetails, Failure>> Fetch(CollectedWeatherDataDetails details);
    Task<OneOf<CollectedWeatherDataDetails, Failure>> CompleteSubmission(CollectedWeatherDataDetails details);

    // Status updates
    Task<OneOf<CollectedWeatherDataDetails, Failure>> UpdateStatusModelingSucceeded(CollectedWeatherDataDetails details);
    Task<OneOf<CollectedWeatherDataDetails, Failure>> UpdateStatusDataRejected(CollectedWeatherDataDetails details);
    Task<OneOf<CollectedWeatherDataDetails, Failure>> UpdateStatusModelUpdated(CollectedWeatherDataDetails details);
    Task<OneOf<CollectedWeatherDataDetails, Failure>> UpdateStatusSubmittedToModeling(CollectedWeatherDataDetails details);
}
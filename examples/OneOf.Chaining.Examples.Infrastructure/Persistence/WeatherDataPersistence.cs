using OneOf.Chaining.Examples.Application.Models;
using OneOf.Chaining.Examples.Application.Orchestration;
using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Infrastructure.Persistence;

public class WeatherDataPersistence : IWeatherDataPersistence
{
    private readonly Dictionary<Guid, CollectedWeatherDataDetails> collectedWeatherRepository = new();
    private readonly List<CollectedWeatherDataDetailsStatusUpdate> collectedWeatherDataStatusUpdateRepository = new();
    
    public async Task<OneOf<CollectedWeatherDataDetails, Failure>> InsertOrFetch(CollectedWeatherDataDetails details)
    {
        if (!collectedWeatherRepository.ContainsKey(details.RequestId))
            collectedWeatherRepository.Add(Guid.NewGuid(), details);

        return OneOf<CollectedWeatherDataDetails, Failure>.FromT0(collectedWeatherRepository[details.RequestId]);
    }

    public async Task<OneOf<CollectedWeatherDataDetails, Failure>> Fetch(CollectedWeatherDataDetails details)
    {
        if (collectedWeatherRepository.ContainsKey(details.RequestId))
        {
            var statusEvents = collectedWeatherDataStatusUpdateRepository.Where(
                e => e.CollectedWeatherDetailsRequestId == details.RequestId);

            var fetchedDetails = collectedWeatherRepository[details.RequestId]
                .WithUpdatedState(statusEvents);

            return OneOf<CollectedWeatherDataDetails, Failure>.FromT0(fetchedDetails); // just return the persisted details object
        }

        throw new Exception($"Could find WeatherData with {details.RequestId} in store."); // this is exceptional
    }

    public Task<OneOf<CollectedWeatherDataDetails, Failure>> CompleteSubmission(CollectedWeatherDataDetails details)
    {
        collectedWeatherDataStatusUpdateRepository.Add(
            new CollectedWeatherDataDetailsStatusUpdate(
                details.RequestId,
                DateTime.Now,
                EventNames.SubmissionComplete.ToString()));

        return Task.FromResult<OneOf<CollectedWeatherDataDetails, Failure>>(details);
    }

    public Task<OneOf<CollectedWeatherDataDetails, Failure>> UpdateStatusAcceptedIntoModeling(CollectedWeatherDataDetails details)
    {
        collectedWeatherDataStatusUpdateRepository.Add(
            new CollectedWeatherDataDetailsStatusUpdate(
                details.RequestId, 
                DateTime.Now, 
                EventNames.ModelingDataAccepted.ToString()));

        return Task.FromResult<OneOf<CollectedWeatherDataDetails, Failure>>(details);
    }

    public Task<OneOf<CollectedWeatherDataDetails, Failure>> UpdateStatusDataRejected(CollectedWeatherDataDetails details)
    {
        collectedWeatherDataStatusUpdateRepository.Add(
            new CollectedWeatherDataDetailsStatusUpdate(
                details.RequestId,
                DateTime.Now,
                EventNames.ModelingDataRejected.ToString()));

        return Task.FromResult<OneOf<CollectedWeatherDataDetails, Failure>>(details);
    }

    public Task<OneOf<CollectedWeatherDataDetails, Failure>> UpdateStatusModelUpdated(CollectedWeatherDataDetails details)
    {
        collectedWeatherDataStatusUpdateRepository.Add(
            new CollectedWeatherDataDetailsStatusUpdate(
                details.RequestId,
                DateTime.Now,
                EventNames.ModelUpdated.ToString()));

        return Task.FromResult<OneOf<CollectedWeatherDataDetails, Failure>>(details);
    }
    
    public Task<OneOf<CollectedWeatherDataDetails, Failure>> UpdateStatusSubmittedToModeling(CollectedWeatherDataDetails details)
    {
        collectedWeatherDataStatusUpdateRepository.Add(
            new CollectedWeatherDataDetailsStatusUpdate(
                details.RequestId,
                DateTime.Now,
                EventNames.SubmittedToModeling.ToString()));

        return Task.FromResult<OneOf<CollectedWeatherDataDetails, Failure>>(details);
    }
}
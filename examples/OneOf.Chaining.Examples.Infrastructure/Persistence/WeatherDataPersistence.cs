using OneOf.Chaining.Examples.Application.Models;
using OneOf.Chaining.Examples.Application.Orchestration;
using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Infrastructure.Persistence;

public class WeatherDataPersistence : IWeatherDataPersistence
{
    public readonly Dictionary<Guid, CollectedWeatherDataDetails> CollectedWeatherRepository = new();
    public readonly List<CollectedWeatherDataDetailsStatusUpdate> CollectedWeatherDataStatusUpdateRepository = new();
    
    public async Task<OneOf<CollectedWeatherDataDetails, Failure>> InsertOrFetch(CollectedWeatherDataDetails details)
    {
        if (!CollectedWeatherRepository.ContainsKey(details.RequestId))
            CollectedWeatherRepository.Add(details.RequestId, details);

        return OneOf<CollectedWeatherDataDetails, Failure>.FromT0(CollectedWeatherRepository[details.RequestId]);
    }

    public async Task<OneOf<CollectedWeatherDataDetails, Failure>> Fetch(CollectedWeatherDataDetails details)
    {
        // todo: fix this - the submissionId is null in the main repo!!! maybe convert to aggregateRoot now?
        var foundCollectedWeatherDetails = CollectedWeatherRepository.First(pair => pair.Value.WeatherModelingServiceSubmissionId == details.WeatherModelingServiceSubmissionId).Value;
       
        var statusEvents = CollectedWeatherDataStatusUpdateRepository.Where(
            e => e.CollectedWeatherDetailsRequestId == details.RequestId);

        return OneOf<CollectedWeatherDataDetails, Failure>.FromT0(foundCollectedWeatherDetails.WithUpdatedState(statusEvents)); // just return the persisted details object
        

        //throw new Exception($"Couldn't find WeatherData with {details.RequestId} in store."); // this is exceptional
    }

    public Task<OneOf<CollectedWeatherDataDetails, Failure>> CompleteSubmission(CollectedWeatherDataDetails details)
    {
        CollectedWeatherDataStatusUpdateRepository.Add(
            new CollectedWeatherDataDetailsStatusUpdate(
                details.RequestId,
                DateTime.Now,
                EventNames.SubmissionComplete.ToString()));

        return Task.FromResult<OneOf<CollectedWeatherDataDetails, Failure>>(details);
    }

    public Task<OneOf<CollectedWeatherDataDetails, Failure>> UpdateStatusAcceptedIntoModeling(CollectedWeatherDataDetails details)
    {
        CollectedWeatherDataStatusUpdateRepository.Add(
            new CollectedWeatherDataDetailsStatusUpdate(
                details.RequestId, 
                DateTime.Now, 
                EventNames.ModelingDataAccepted.ToString()));

        return Task.FromResult<OneOf<CollectedWeatherDataDetails, Failure>>(details);
    }

    public Task<OneOf<CollectedWeatherDataDetails, Failure>> UpdateStatusDataRejected(CollectedWeatherDataDetails details)
    {
        CollectedWeatherDataStatusUpdateRepository.Add(
            new CollectedWeatherDataDetailsStatusUpdate(
                details.RequestId,
                DateTime.Now,
                EventNames.ModelingDataRejected.ToString()));

        return Task.FromResult<OneOf<CollectedWeatherDataDetails, Failure>>(details);
    }

    public Task<OneOf<CollectedWeatherDataDetails, Failure>> UpdateStatusModelUpdated(CollectedWeatherDataDetails details)
    {
        CollectedWeatherDataStatusUpdateRepository.Add(
            new CollectedWeatherDataDetailsStatusUpdate(
                details.RequestId,
                DateTime.Now,
                EventNames.ModelUpdated.ToString()));

        return Task.FromResult<OneOf<CollectedWeatherDataDetails, Failure>>(details);
    }
    
    public Task<OneOf<CollectedWeatherDataDetails, Failure>> UpdateStatusSubmittedToModeling(CollectedWeatherDataDetails details)
    {
        CollectedWeatherDataStatusUpdateRepository.Add(
            new CollectedWeatherDataDetailsStatusUpdate(
                details.RequestId,
                DateTime.Now,
                EventNames.SubmittedToModeling.ToString(), details.WeatherModelingServiceSubmissionId!.ToString()));

        return Task.FromResult<OneOf<CollectedWeatherDataDetails, Failure>>(details);
    }
}
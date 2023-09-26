using OneOf.Chaining.Examples.Application.Orchestration;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Services;

public class WeatherDataPersistence : IWeatherDataPersistence
{
    private readonly Dictionary<Guid, CollectedWeatherDataDetails> persistedWeatherData = new();
    
    public async Task<OneOf<CollectedWeatherDataDetails, Failure>> InsertOrFetch(CollectedWeatherDataDetails details)
    {
        if (!persistedWeatherData.ContainsKey(details.RequestId))
            persistedWeatherData.Add(Guid.NewGuid(), details);

        return OneOf<CollectedWeatherDataDetails, Failure>.FromT0(persistedWeatherData[details.RequestId]);
    }

    public async Task<OneOf<CollectedWeatherDataDetails, Failure>> Fetch(CollectedWeatherDataDetails details)
    {
        if (persistedWeatherData.ContainsKey(details.RequestId))
        {
            return OneOf<CollectedWeatherDataDetails, Failure>.FromT0(persistedWeatherData[details.RequestId]);
        }

        throw new Exception($"Could find WeatherData with {details.RequestId} in store.");
    }

    public Task<OneOf<CollectedWeatherDataDetails, Failure>> CompleteSubmission(CollectedWeatherDataDetails details)
    {
        throw new NotImplementedException();
    }

    public Task<OneOf<CollectedWeatherDataDetails, Failure>> UpdateStatusModelingSucceeded(CollectedWeatherDataDetails details)
    {
        throw new NotImplementedException();
    }

    public Task<OneOf<CollectedWeatherDataDetails, Failure>> UpdateStatusDataRejected(CollectedWeatherDataDetails details)
    {
        throw new NotImplementedException();
    }

    public Task<OneOf<CollectedWeatherDataDetails, Failure>> UpdateStatusModelUpdated(CollectedWeatherDataDetails details)
    {
        throw new NotImplementedException();
    }
    
    public Task<OneOf<CollectedWeatherDataDetails, Failure>> UpdateStatusSubmittedToModeling(CollectedWeatherDataDetails details)
    {
        throw new NotImplementedException();
    }
}

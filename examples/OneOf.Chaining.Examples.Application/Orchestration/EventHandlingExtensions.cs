using OneOf.Chaining.Examples.Domain.Entities;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Orchestration;

public static class EventHandlingExtensions
{
    public static async Task ThrowOnFailure(this Task<OneOf<WeatherDataCollection, Failure>> successOrFailure, string eventName)
    {
        var result = await successOrFailure;
        if (result.IsT1)
            throw new Exception($"Something went wrong while handling {eventName}");
    }
}
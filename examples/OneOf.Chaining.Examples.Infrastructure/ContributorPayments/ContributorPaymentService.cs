using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain.Entities;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Infrastructure.ContributorPayments;

public class ContributorPaymentService : IContributorPaymentService
{
    public Task<OneOf<WeatherDataCollection, Failure>> CreatePendingPayment(WeatherDataCollection weatherDataCollection)
    {
        // todo: add a refit client?
        return Task.FromResult(OneOf<WeatherDataCollection, Failure>.FromT0(weatherDataCollection));
    }

    public Task<OneOf<WeatherDataCollection, Failure>> RevokePendingPayment(WeatherDataCollection weatherDataCollection)
    {
        return Task.FromResult(OneOf<WeatherDataCollection, Failure>.FromT0(weatherDataCollection));
    }

    public Task<OneOf<WeatherDataCollection, Failure>> CommitPendingPayment(WeatherDataCollection weatherDataCollection)
    {
        return Task.FromResult(OneOf<WeatherDataCollection, Failure>.FromT0(weatherDataCollection));
    }
}
using OneOf.Chaining.Examples.Application.Orchestration;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Services;

public class ContributorPaymentService : IContributorPaymentService
{
    public Task<OneOf<CollectedWeatherDataDetails, Failure>> CreatePendingPayment(CollectedWeatherDataDetails details)
    {
        throw new NotImplementedException();
    }

    Task<OneOf<CollectedWeatherDataDetails, Failure>> IContributorPaymentService.RevokePendingPayment(CollectedWeatherDataDetails details)
    {
        throw new NotImplementedException();
    }

    Task<OneOf<CollectedWeatherDataDetails, Failure>> IContributorPaymentService.CommitPendingPayment(CollectedWeatherDataDetails details)
    {
        throw new NotImplementedException();
    }
}
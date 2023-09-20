using OneOf.Chaining.Examples.Application.Orchestration;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Services;

public interface IContributorPaymentService
{
    Task<OneOf<CollectedWeatherDataDetails, Failure>> CreatePendingPayment(CollectedWeatherDataDetails details);
    Task<OneOf<CollectedWeatherDataDetails, Failure>> RevokePendingPayment(CollectedWeatherDataDetails details);
    Task<OneOf<CollectedWeatherDataDetails, Failure>> CommitPendingPayment(CollectedWeatherDataDetails details);
}
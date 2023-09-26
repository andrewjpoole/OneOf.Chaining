using OneOf.Chaining.Examples.Application.Orchestration;
using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Infrastructure.ContributorPayments
{
    public class ContributorPaymentService : IContributorPaymentService
    {
        public Task<OneOf<CollectedWeatherDataDetails, Failure>> CreatePendingPayment(CollectedWeatherDataDetails details)
        {
            throw new NotImplementedException();
        }

        public Task<OneOf<CollectedWeatherDataDetails, Failure>> RevokePendingPayment(CollectedWeatherDataDetails details)
        {
            throw new NotImplementedException();
        }

        public Task<OneOf<CollectedWeatherDataDetails, Failure>> CommitPendingPayment(CollectedWeatherDataDetails details)
        {
            throw new NotImplementedException();
        }
    }
}

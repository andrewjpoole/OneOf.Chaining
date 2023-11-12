using OneOf.Chaining.Examples.Domain.Entities;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Services;

public interface IContributorPaymentService
{
    Task<OneOf<WeatherDataCollection, Failure>> CreatePendingPayment(
        WeatherDataCollection weatherDataCollection);
    Task<OneOf<WeatherDataCollection, Failure>> RevokePendingPayment(
        WeatherDataCollection weatherDataCollection);
    Task<OneOf<WeatherDataCollection, Failure>> CommitPendingPayment(
        WeatherDataCollection weatherDataCollection);
}
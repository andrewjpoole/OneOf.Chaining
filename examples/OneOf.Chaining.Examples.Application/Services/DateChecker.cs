using OneOf.Chaining.Examples.Domain;
using OneOf.Chaining.Examples.Domain.Outcomes;
#pragma warning disable CS1998

namespace OneOf.Chaining.Examples.Application.Services;

public class DateChecker : IDateChecker
{
    public async Task<OneOf<WeatherReportDetails, Failure>> CheckDate(WeatherReportDetails report)
    {
        if (report.RequestedDate < DateTime.Today)
            return OneOf<WeatherReportDetails, Failure>.FromT1(
                new InvalidRequestFailure(new Dictionary<string, string[]>{ { "Date", new [] {"Date must be in next two weeks" } } }));

        if (report.RequestedDate > DateTime.Today.AddDays(14))
            return OneOf<WeatherReportDetails, Failure>.FromT1(
                new InvalidRequestFailure(new Dictionary<string, string[]> { { "Date", new[] { "Date must be in next two weeks" } } }));

        return report;
    }
}

public interface IDateChecker
{
    Task<OneOf<WeatherReportDetails, Failure>> CheckDate(WeatherReportDetails report);
}
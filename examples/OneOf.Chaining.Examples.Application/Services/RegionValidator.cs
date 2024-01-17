using OneOf.Chaining.Examples.Domain;
using OneOf.Chaining.Examples.Domain.Outcomes;
#pragma warning disable CS1998

namespace OneOf.Chaining.Examples.Application.Services;

public class RegionValidator : IRegionValidator
{
    private readonly List<string> supportedRegions = ["taunton", "bristol", "london"];

    public async Task<OneOf<WeatherReportDetails, Failure>> ValidateRegion(WeatherReportDetails report)
    {
        if (supportedRegions.Contains(report.RequestedRegion.ToLower()))
            return report;

        return OneOf<WeatherReportDetails, Failure>.FromT1(new UnsupportedRegionFailure(report.RequestedRegion));
    }
}

public interface IRegionValidator
{
    Task<OneOf<WeatherReportDetails, Failure>> ValidateRegion(WeatherReportDetails report);
}


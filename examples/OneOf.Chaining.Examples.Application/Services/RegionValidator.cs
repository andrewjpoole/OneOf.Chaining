using OneOf.Chaining.Examples.Domain;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Services;

public class RegionValidator : IRegionValidator
{
    private List<string> supportedRegions = new() { "taunton", "bristol", "london" };

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


using OneOf.Chaining.Examples.Domain;

namespace OneOf.Chaining.Examples.Application.Models;

public record WeatherReportResponse(string RequestedRegion, DateTime RequestedDate, Guid RequestId, int Temperature, string Summary)
{
    public static WeatherReportResponse FromDetails(WeatherReportDetails details)
    {
        return new WeatherReportResponse(details.RequestedRegion, details.RequestedDate, details.RequestId, details.Temperature, details.Summary);
    }
}
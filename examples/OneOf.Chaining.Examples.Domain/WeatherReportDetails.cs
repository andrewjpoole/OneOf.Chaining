using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Domain;

public class WeatherReportDetails
{
    public string RequestedRegion { get; }
    public DateTime RequestedDate { get; }
    public Guid RequestId { get; }

    public bool PopulatedFromCache { get; set; }
    public int Temperature { get; private set; }
    public string Summary { get; private set; } = "";

    private WeatherReportDetails(string requestedRegion, DateTime requestedDate)
    {
        RequestedRegion = requestedRegion;
        RequestedDate = requestedDate;
        RequestId = Guid.NewGuid();
    }

    public static Task<OneOf<WeatherReportDetails, Failure>> Create(string requestedRegion, DateTime requestedDate)
    {
        return Task.FromResult(
            OneOf<WeatherReportDetails, Failure>.FromT0(
                new WeatherReportDetails(requestedRegion, requestedDate)));
    }

    public void Set(string summary, int temperature)
    {
        Summary = summary;
        Temperature = temperature;
    }
}
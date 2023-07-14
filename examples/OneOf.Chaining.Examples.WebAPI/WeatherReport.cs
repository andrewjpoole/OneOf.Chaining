using OneOf;

public class WeatherReport
{
    public string RequestedRegion { get; }
    public DateTime RequestedDate { get; }
    public Guid RequestId { get; }

    public int Temperature { get; private set; }
    public string Summary { get; private set; }

    private WeatherReport(string requestedRegion, DateTime requestedDate)
    {
        RequestedRegion = requestedRegion;
        RequestedDate = requestedDate;
        RequestId = Guid.NewGuid();
    }

    public static Task<OneOf<WeatherReport, Failure>> Create(string requestedRegion, DateTime requestedDate)
    {
        return Task.FromResult(OneOf<WeatherReport, Failure>.FromT0(new WeatherReport(requestedRegion, requestedDate)));
    }

    public void Set(string summary, int temperature)
    {
        Summary = summary;
        Temperature = temperature;
    }
}
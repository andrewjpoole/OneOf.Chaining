namespace OneOf.Chaining.Examples.Domain.Outcomes;

public class WeatherModelingServiceRejectionFailure
{
    public string Message { get; }

    public WeatherModelingServiceRejectionFailure(string message)
    {
        Message = message;
    }
}
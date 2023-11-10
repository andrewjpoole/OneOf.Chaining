namespace OneOf.Chaining.Examples.Domain.Outcomes;

public class Failure : OneOfBase<InvalidRequestFailure, UnsupportedRegionFailure, WeatherModelingServiceRejectionFailure>
{
    public Failure(OneOf<InvalidRequestFailure, UnsupportedRegionFailure, WeatherModelingServiceRejectionFailure> input) : base(input)
    {
    }

    public static implicit operator Failure(InvalidRequestFailure failure) => new(failure);
    public static implicit operator Failure(UnsupportedRegionFailure failure) => new(failure);
    public static implicit operator Failure(WeatherModelingServiceRejectionFailure failure) => new(failure);
}
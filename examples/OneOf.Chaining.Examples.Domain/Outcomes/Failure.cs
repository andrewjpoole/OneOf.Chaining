namespace OneOf.Chaining.Examples.Domain.Outcomes;

public class Failure : OneOfBase<InvalidRequestFailure, UnsupportedRegionFailure>
{
    public Failure(OneOf<InvalidRequestFailure, UnsupportedRegionFailure> input) : base(input)
    {
    }

    public static implicit operator Failure(InvalidRequestFailure _) => new(_);
    public static implicit operator Failure(UnsupportedRegionFailure _) => new(_);
}
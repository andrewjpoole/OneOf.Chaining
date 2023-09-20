namespace OneOf.Chaining.Examples.Domain.Outcomes;

public class Failure : OneOfBase<InvalidRequest, UnsupportedRegionFailure>
{
    public Failure(OneOf<InvalidRequest, UnsupportedRegionFailure> input) : base(input)
    {
    }

    public static implicit operator Failure(InvalidRequest _) => new(_);
    public static implicit operator Failure(UnsupportedRegionFailure _) => new(_);
}
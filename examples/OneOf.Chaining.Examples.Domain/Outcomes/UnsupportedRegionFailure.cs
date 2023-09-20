namespace OneOf.Chaining.Examples.Domain.Outcomes;

public class UnsupportedRegionFailure
{
    public string Title => "Unsupported Region";

    public string Detail { get; }

    public UnsupportedRegionFailure(string region)
    {
        Detail = $"{region} is not a supported region";
    }
}
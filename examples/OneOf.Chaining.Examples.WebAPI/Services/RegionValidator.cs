namespace OneOf.Chaining.Examples.WebAPI.Services
{
    public class RegionValidator : IRegionValidator
    {
        private List<string> supportedRegions = new() { "taunton", "bristol", "london" };

        public async Task<OneOf<WeatherReport, Failure>> ValidateRegion(WeatherReport report)
        {

            if (supportedRegions.Contains(report.RequestedRegion.ToLower()))
                return report;

            return OneOf<WeatherReport, Failure>.FromT1(new UnsupportedRegionFailure("Unsupported Region", $"{report.RequestedRegion} is not supported"));
        }
    }
}

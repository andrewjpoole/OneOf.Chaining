using OneOf.Chaining.Examples.WebAPI.Services;

namespace OneOf.Chaining.Examples.WebAPI.Handlers
{
    public class GetWeatherReportRequestHandler : IGetWeatherReportRequestHandler
    {
        private readonly IRegionValidator regionValidator;
        private readonly IDateChecker dateChecker;
        private readonly IWeatherForecastGenerator weatherForecastGenerator;

        public GetWeatherReportRequestHandler(IRegionValidator regionValidator, IDateChecker dateChecker, IWeatherForecastGenerator weatherForecastGenerator)
        {
            this.regionValidator = regionValidator;
            this.dateChecker = dateChecker;
            this.weatherForecastGenerator = weatherForecastGenerator;
        }

        public async Task<OneOf<WeatherReport, Failure>> Handle(string requestedRegion, DateTime requestedDate)
        {
            return await WeatherReport.Create(requestedRegion, requestedDate)
                .Then(report => regionValidator.ValidateRegion(report))
                .Then(report => dateChecker.CheckDate(report))
                .Then(report => CheckCache(report))
                .Then(report => weatherForecastGenerator.Generate(report));
        }

        public async Task<OneOf<WeatherReport, Failure>> CheckCache(WeatherReport report)
        {
            // Check and populate from a cache etc...
            // Methods from anywhere can be chained as long as they have the correct signature...
            return report;
        }
    }
}

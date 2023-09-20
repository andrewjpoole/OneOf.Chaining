using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Handlers;

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

    /*
    public async Task<OneOf<WeatherReport, Failure>> Handle(string requestedRegion, DateTime requestedDate)
    {
        var isValidRequest = await regionValidator.Validate(requestedRegion);
        if (!isValidRequest)
            return new UnsupportedRegionFailure();

        var dateCheckPassed = await dateChecker.CheckDate(requestedDate);
        if (!dateCheckPassed)
            return new InvalidRequest();

        var cacheCheckResult = CheckCache(requestedRegion, requestedDate);
        if (cacheCheckResult.Hit)
            return cacheCheckResult.Data;
        else
            return weatherForecastGenerator.Generate(requestedRegion, requestedDate);
    }



























    */
    
    public async Task<OneOf<WeatherReport, Failure>> Handle(string requestedRegion, DateTime requestedDate)
    {
        return await WeatherReport.Create(requestedRegion, requestedDate)
            .Then(regionValidator.ValidateRegion)
            .Then(dateChecker.CheckDate)
            .Then(CheckCache)
            .Then(weatherForecastGenerator.Generate);
    }

    public async Task<OneOf<WeatherReport, Failure>> CheckCache(WeatherReport report)
    {
        // Check and populate from a cache etc...
        // Methods from anywhere can be chained as long as they have the correct signature...

        await Task.Delay(50);
        return report;
        
        //cacheId = "sdfsdf";
        //return Task.FromResult(OneOf<WeatherReport, Failure>.FromT0(report));
    }



    /*
     * Demonstrate:
     * extra args
     * out args if method not async
     * go through Then extension method code
     * onFailure - shown later
     */
}
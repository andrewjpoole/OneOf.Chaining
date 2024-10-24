﻿using OneOf.Chaining.Examples.Application.Models;
using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.Application.Handlers;

public class GetWeatherReportRequestHandler : IGetWeatherReportRequestHandler
{
    private readonly IRegionValidator regionValidator;
    private readonly IDateChecker dateChecker;
    private readonly IWeatherForecastGenerator weatherForecastGenerator;

    public GetWeatherReportRequestHandler(
        IRegionValidator regionValidator, 
        IDateChecker dateChecker, 
        IWeatherForecastGenerator weatherForecastGenerator)
    {
        this.regionValidator = regionValidator;
        this.dateChecker = dateChecker;
        this.weatherForecastGenerator = weatherForecastGenerator;
    }

    /*
    // Example of what we didn't want...
    public async Task<OneOf<WeatherReport, Failure>> HandleGetWeatherReport(
        string requestedRegion, DateTime requestedDate)
    {
        var isValidRequest = await regionValidator.Validate(requestedRegion);
        if (!isValidRequest)
            return new UnsupportedRegionFailure();

        var dateCheckPassed = await dateChecker.CheckDate(requestedDate);
        if (!dateCheckPassed)
            return new InvalidRequestFailure();

        var cacheCheckResult = CheckCache(requestedRegion, requestedDate);
        if (cacheCheckResult.Hit)
            return cacheCheckResult.Data;
        else
            return weatherForecastGenerator.Generate(requestedRegion, requestedDate);
    }



























    */
    
    public async Task<OneOf<WeatherReportResponse, Failure>> HandleGetWeatherReport(
        string requestedRegion, DateTime requestedDate)
    {
        var settings = "34h5gm4h5g"; // An example of a variable in scope that can be captured in the lambda and passed into a chained method below...
        return await WeatherReportDetails.Create(requestedRegion, requestedDate)
            .Then(regionValidator.ValidateRegion)
            .Then(dateChecker.CheckDate)
            .Then(d => CheckCache(d, settings))
            .IfThen(d => d.PopulatedFromCache is false, 
                weatherForecastGenerator.Generate)
            .ToResult(WeatherReportResponse.FromDetails);
    }
/*











*/
    public async Task<OneOf<WeatherReportDetails, Failure>> CheckCache(
        WeatherReportDetails details, string settings)
    {
        // Check and populate from a local in-memory cache etc...
        // Methods from anywhere can be chained as long as they
        // have the correct return type, matching the T and TFailure for the chain...
        
        await Task.Delay(50);
        details.Set("summary from cache", 32);
        details.PopulatedFromCache = true;
        
        return details;
    }



/* Notes for talk demo...
 * 1. details object passing through on success (containing state)
 * 2. add CheckCache() show actual details arg without MethodGrouping
 * 3. add an additional arg
 * 4. mention: navigation, debugging, testing
 * 5. go through Then extension method code
 * 6. IfThen .IfThen(report => report.PopulatedFromCache is false, 
 * 7. onFailure
 */
}
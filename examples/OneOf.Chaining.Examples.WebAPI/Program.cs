using Microsoft.AspNetCore.Mvc;
using OneOf.Chaining.Examples.Application.Handlers;
using OneOf.Chaining.Examples.Application.Models.Requests;
using OneOf.Chaining.Examples.Application.Orchestration;
using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain.Outcomes;
using OneOf.Chaining.Examples.Domain.ServiceDefinitions;
using OneOf.Chaining.Examples.Infrastructure.ApiClients;
using OneOf.Chaining.Examples.Infrastructure.ApiClients.WeatherModelingSystem;
using OneOf.Chaining.Examples.Infrastructure.ContributorPayments;
using OneOf.Chaining.Examples.Infrastructure.LocationManager;
using OneOf.Chaining.Examples.Infrastructure.Notifications;
using OneOf.Chaining.Examples.Infrastructure.Persistence;

namespace OneOf.Chaining.Examples.WebAPI;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container

        builder.Services
            .AddSingleton<IGetWeatherReportRequestHandler, GetWeatherReportRequestHandler>()
            .AddSingleton<IPostWeatherReportDataHandler, CollectedWeatherDataOrchestrator>()
            .AddSingleton<IRegionValidator, RegionValidator>()
            .AddSingleton<IDateChecker, DateChecker>()
            .AddSingleton<IWeatherForecastGenerator, WeatherForecastGenerator>()
            .AddSingleton<IEventPersistenceService, EventPersistenceService>()
            .AddSingleton<IEventRepository, EventRepository>()
            .AddSingleton<INotificationService, NotificationService>()
            .AddSingleton<IWeatherDataValidator, WeatherDataValidator>()
            .AddSingleton<ILocationManager, LocationManager>()
            .AddSingleton<IContributorPaymentService, ContributorPaymentService>()
            .AddWeatherModelingService(builder.Configuration.GetSection(WeatherModelingServiceOptions.ConfigSectionName).Get<WeatherModelingServiceOptions>())
            .AddSingleton(typeof(IRefitClientWrapper<>), typeof(RefitClientWrapper<>));

        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseHttpsRedirection();

        app.MapGet("/v1/weather-forecast/{region}/{date}", (
            [FromRoute] string region,
            [FromRoute] DateTime date,
            [FromServices] IGetWeatherReportRequestHandler handler)
            => CreateResponseFor(() => handler.Handle(region, date)));

        #region
        app.MapPost("/v1/collected-weather-data/{location}", (
            [FromRoute] string location,
            [FromBody] CollectedWeatherDataModel data,
            [FromServices] IPostWeatherReportDataHandler handler,
            [FromServices] IWeatherDataValidator weatherDataValidator,
            [FromServices] ILocationManager locationManager)
            => CreateResponseFor(() => handler.Handle(location, data, weatherDataValidator, locationManager)));
        #endregion

        static async Task<IResult> CreateResponseFor<TSuccess>(Func<Task<OneOf<TSuccess, Failure>>> handleRequestFunc)
        {
            var result = await handleRequestFunc.Invoke();
            return result.Match(
                success => Results.Ok(success),
                failure => failure.Match(
                    invalidRequestFailure => Results.BadRequest(new ValidationProblemDetails(invalidRequestFailure.ValidationErrors)),
                    unsupportedRegionFailure => Results.UnprocessableEntity(unsupportedRegionFailure.ToProblemDetails()),
                    weatherModelingServiceRejectionFailure => Results.UnprocessableEntity(weatherModelingServiceRejectionFailure.Message)
                ));
        }
        
        await app.RunAsync();
    }
}

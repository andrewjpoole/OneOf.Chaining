using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Chaining.Examples.Application.Handlers;
using OneOf.Chaining.Examples.Application.Models.Requests;
using OneOf.Chaining.Examples.Application.Orchestration;
using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain.Outcomes;
using OneOf.Chaining.Examples.WebAPI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services
    .AddSingleton<IGetWeatherReportRequestHandler, GetWeatherReportRequestHandler>()
    .AddSingleton<IRegionValidator, RegionValidator>()
    .AddSingleton<IDateChecker, DateChecker>()
    .AddSingleton<IWeatherForecastGenerator, WeatherForecastGenerator>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/weatherforecast/{region}/{date}", (
    [FromRoute] string region,
    [FromRoute] DateTime date,
    [FromServices] IGetWeatherReportRequestHandler handler)
    => CreateResponseFor(() => handler.Handle(region, date)));

app.MapPost("/collectedweatherdata/{location}", (
    [FromRoute] string location,
    [FromBody] CollectedWeatherDataModel data,
    [FromServices] IPostWeatherReportDataHandler handler,
    [FromServices] IWeatherDataValidator weatherDataValidator,
    [FromServices] ILocationManager locationManager)
    => CreateResponseFor(() => handler.Handle(location, data, weatherDataValidator, locationManager)));

static async Task<IResult> CreateResponseFor<TSuccess>(Func<Task<OneOf<TSuccess, Failure>>> handleRequestFunc)
{
    var response = await handleRequestFunc.Invoke();
    return response.Match(
        success => Results.Ok(success),
        failure => failure.Match(
            invalidRequest =>
            {
                var problem = new ValidationProblemDetails(invalidRequest.ValidationErrors);
                return Results.BadRequest(problem);
            },
            unsupportedRegionFailure => Results.UnprocessableEntity(unsupportedRegionFailure.ToProblemDetails())
        ));
}

app.Run();

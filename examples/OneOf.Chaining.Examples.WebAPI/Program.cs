using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Chaining.Examples.WebAPI.Handlers;
using OneOf.Chaining.Examples.WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services
    .AddSingleton<IGetWeatherReportRequestHandler, GetWeatherReportRequestHandler>()
    .AddSingleton<IRegionValidator, RegionValidator>()
    .AddSingleton<IDateChecker, DateChecker>()
    .AddSingleton<IWeatherForecastGenerator, WeatherForecastGenerator>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();



app.MapGet("/weatherforecast/{region}/{date}", (
    [FromRoute]string region, 
    [FromRoute]DateTime date, 
    [FromServices]IGetWeatherReportRequestHandler handler) => CreateResponseFor(() => handler.Handle(region, date)));


static async Task<IResult> CreateResponseFor(Func<Task<OneOf<WeatherReport, Failure>>> handleRequestFunc)
{
    var response = await handleRequestFunc.Invoke();
    return response.Match(
        successfulReport => Results.Ok(successfulReport),
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


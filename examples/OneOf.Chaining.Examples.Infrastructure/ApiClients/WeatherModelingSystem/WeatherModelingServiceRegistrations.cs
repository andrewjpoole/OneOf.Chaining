using Microsoft.Extensions.DependencyInjection;
using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Infrastructure.ModelingService;
using Polly;

namespace OneOf.Chaining.Examples.Infrastructure.ApiClients.WeatherModelingSystem;

public static class WeatherModelingServiceRegistrations
{
    public static IServiceCollection AddWeatherModelingService(this IServiceCollection services, WeatherModelingServiceOptions weatherModelingServiceApiOptions)
    {
        var typeOfClientInterface = typeof(WeatherModelingService);
        var nameOfClientInterface = typeOfClientInterface.FullName ?? typeOfClientInterface.Name;
        services.AddHttpClient(nameOfClientInterface, client =>
            {
                if (string.IsNullOrWhiteSpace(weatherModelingServiceApiOptions.BaseUrl))
                    throw new Exception($"{nameof(weatherModelingServiceApiOptions.BaseUrl)} is required on {nameof(weatherModelingServiceApiOptions)} section in config.");

                client.BaseAddress = new Uri(weatherModelingServiceApiOptions.BaseUrl);
                client.DefaultRequestHeaders.Add(weatherModelingServiceApiOptions.ApiManagerSubscriptionKeyHeader, weatherModelingServiceApiOptions.SubscriptionKey);
            })
            .AddTransientHttpErrorPolicy(policy =>
                policy.WaitAndRetryAsync(weatherModelingServiceApiOptions.MaxRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

        services.AddSingleton<IWeatherModelingService, WeatherModelingService>();

        return services;
    }
}
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Infrastructure.ApiClients.WeatherModelingSystem;
using OneOf.Chaining.Examples.Infrastructure.Persistence;

namespace OneOf.Chaining.Examples.Tests.Framework;

public class ApiWebApplicationFactory : WebApplicationFactory<WebAPI.Program>
{
    public HttpClient? HttpClient;

    public readonly Mock<ILogger> MockLogger = new();
    public readonly Mock<HttpMessageHandler> MockWeatherModelingServiceHttpMessageHandler = new();

    public Func<WeatherDataPersistence>? SetSharedPersistence = null;

    // Using CreateHost here instead of ConfigureWebHost because CreateHost adds config just after WebApplication.CreateBuilder(args) is called
    // whereas ConfigureWebHost is called too late just before builder.Build() is called.
    protected override IHost CreateHost(IHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("WeatherModelingServiceOptions__BaseUrl", Constants.WeatherModelingServiceBaseUrl);
        Environment.SetEnvironmentVariable("WeatherModelingServiceOptions__MaxRetryCount", "3");

        builder
            .ConfigureServices(services =>
            {
                var loggerFactory = new Mock<ILoggerFactory>();
                loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(MockLogger.Object);
                services.AddSingleton(loggerFactory.Object);

                services.AddHttpClient(typeof(IWeatherModelingServiceClient).FullName!, client => client.BaseAddress = new Uri(Constants.WeatherModelingServiceBaseUrl))
                    .ConfigurePrimaryHttpMessageHandler(() => MockWeatherModelingServiceHttpMessageHandler.Object);

                //services.AddSingleton<IDbConnectionFactory>(provider => MockDbConnectionFactory.Object);

                if(SetSharedPersistence is not null)
                    services.AddSingleton<IWeatherDataPersistence>(_ => SetSharedPersistence());
            });

        var host = base.CreateHost(builder);

        //RealWeatherDataPersistence = (WeatherDataPersistence)host.Services.GetService<IWeatherDataPersistence>()!; // todo: add an underlying layer to mock in future.

        return host;
    }

    public void Start()
    {
        HttpClient = CreateClient();
    }
}
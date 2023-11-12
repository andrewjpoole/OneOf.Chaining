using Microsoft.Extensions.Options;
using OneOf.Chaining.Examples.Application.Handlers;
using OneOf.Chaining.Examples.Application.Models.IntegrationEvents.WeatherModelingEvents;
using OneOf.Chaining.Examples.Application.Orchestration;
using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain.ServiceDefinitions;
using OneOf.Chaining.Examples.Infrastructure.ApiClients.WeatherModelingSystem;
using OneOf.Chaining.Examples.Infrastructure.ContributorPayments;
using OneOf.Chaining.Examples.Infrastructure.LocationManager;
using OneOf.Chaining.Examples.Infrastructure.MessageBus;
using OneOf.Chaining.Examples.Infrastructure.Notifications;
using OneOf.Chaining.Examples.Infrastructure.Persistence;

namespace OneOf.Chaining.Examples.EventListener.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var config = builder.Configuration;

        //services.Configure<ServiceBusInboundQueueHandlerOptions>(config.GetSection(ServiceBusInboundQueueHandlerOptions.Name));

        var queueHandlerOptions = config.GetSection(ServiceBusInboundQueueHandlerOptions.Name).Get<ServiceBusInboundQueueHandlerOptions>();
        IOptions<ServiceBusOptions> inboundServiceBusOptions = Options.Create(queueHandlerOptions);
        services.AddSingleton(inboundServiceBusOptions);

        services.ConfigureServiceBusClient(config);
        
        services
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

        services.AddHostedServiceBusEventListener<ModelingDataAcceptedEvent, CollectedWeatherDataOrchestrator>();
        services.AddHostedServiceBusEventListener<ModelingDataRejectedEvent, CollectedWeatherDataOrchestrator>();
        services.AddHostedServiceBusEventListener<ModelUpdatedEvent, CollectedWeatherDataOrchestrator>();
    }
}
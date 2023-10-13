﻿using OneOf.Chaining.Examples.Application.Models.Events.WeatherModelingEvents;
using OneOf.Chaining.Examples.Application.Orchestration;
using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Infrastructure.ContributorPayments;
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

        services.Configure<ServiceBusInboundQueueHandlerOptions>(config.GetSection(ServiceBusInboundQueueHandlerOptions.Name));
        
        services.ConfigureServiceBusClient(config);
        
        services
            .AddSingleton<IWeatherDataPersistence, WeatherDataPersistence>()
            .AddSingleton<IContributorPaymentService, ContributorPaymentService>()
            .AddSingleton<INotificationService, NotificationService>();

        services.AddHostedServiceBusEventListener<ModelingDataAcceptedEvent, CollectedWeatherDataOrchestrator>();
        services.AddHostedServiceBusEventListener<ModelingDataRejectedEvent, CollectedWeatherDataOrchestrator>();
        services.AddHostedServiceBusEventListener<ModelUpdatedEvent, CollectedWeatherDataOrchestrator>();

        
    }
}
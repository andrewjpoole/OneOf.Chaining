using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneOf.Chaining.Examples.Application.Services;

namespace OneOf.Chaining.Examples.Infrastructure.MessageBus;

public static class EventListenerRegistrations
{
    public static IServiceCollection AddHostedServiceBusEventListener<T, THandler>(this IServiceCollection services)
        where T : class
        where THandler : IEventHandler<T>
    {
        services.AddSingleton(typeof(IEventHandler<T>), typeof(THandler));
        services.AddHostedService<ServiceBusEventListener<T>>();

        return services;
    }

    public static IServiceCollection ConfigureServiceBusClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAzureClients(builder =>
        {
            var serviceBusConnectionString = configuration.GetConnectionString("ServiceBus");
            builder.AddServiceBusClient(serviceBusConnectionString);
        });

        return services;
    }
}
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using OneOf.Chaining.Examples.Application.Models.IntegrationEvents.WeatherModelingEvents;
using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Infrastructure.Persistence;

namespace OneOf.Chaining.Examples.Tests.Framework;

public class EventListenerWebApplicationFactory : WebApplicationFactory<EventListener.Program>
{
    public int NumberOfSimulatedServiceBusMessageRetries = 3;
    public DictionaryByType TestableServiceBusProcessors { get; } = new();
    public readonly Mock<ILogger> MockLogger = new();
    //public readonly List<PersistedEvent> PersistedEvents = new();

    public Func<EventRepository>? SetSharedEventRepository = null;

    public HttpClient? HttpClient;

#if DEBUG
    private readonly string localMachineNamePrefix = $"{Environment.MachineName}-";
#else
    private string localMachineNamePrefix = "";
#endif

    public const string TestModelingDataAcceptedEventQueueName = "TestModelingDataAcceptedEventQueueName";
    public const string TestModelingDataRejectedEventQueueName = "TestModelingDataRejectedEventQueueName";
    public const string TestModelUpdatedEventQueueName = "TestModelUpdatedEventQueueName";

    // Using CreateHost here instead of ConfigureWebHost because CreateHost adds config just after WebApplication.CreateBuilder(args) is called
    // whereas ConfigureWebHost is called too late just before builder.Build() is called
    protected override IHost CreateHost(IHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ServiceBus__Inbound__Names__ModelingDataAcceptedIntegrationEvent", TestModelingDataAcceptedEventQueueName);
        Environment.SetEnvironmentVariable("ServiceBus__Inbound__Names__ModelingDataRejectedIntegrationEvent", TestModelingDataRejectedEventQueueName);
        Environment.SetEnvironmentVariable("ServiceBus__Inbound__Names__ModelUpdatedIntegrationEvent", TestModelUpdatedEventQueueName);
        Environment.SetEnvironmentVariable("ServiceBus__Inbound__MaxConcurrentCalls", "1");
        Environment.SetEnvironmentVariable("ServiceBus__Inbound__InitialBackoffInMs", "2000");
        Environment.SetEnvironmentVariable("ServiceBus__Inbound__PrefetchCount", "1");
        Environment.SetEnvironmentVariable("ServiceBusSettings__FullyQualifiedNamespace", "component-test-servicebus-namespace");

        builder
            .ConfigureServices(services =>
            {
                var loggerFactory = new Mock<ILoggerFactory>();
                loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(MockLogger.Object);
                services.AddSingleton(loggerFactory.Object);

                var client = new Mock<ServiceBusClient>();
                client.Setup(t => t.CreateProcessor(It.IsAny<string>(), It.IsAny<ServiceBusProcessorOptions>())).Returns((string queue, ServiceBusProcessorOptions _) =>
                {
                    if (queue == $"{localMachineNamePrefix}{TestModelingDataAcceptedEventQueueName}")
                    {
                        var testableProcessor = new TestableServiceBusProcessor<ModelingDataAcceptedIntegrationEvent>();
                        TestableServiceBusProcessors.Add(testableProcessor);
                        return testableProcessor;
                    }

                    if (queue == $"{localMachineNamePrefix}{TestModelingDataRejectedEventQueueName}")
                    {
                        var testableProcessor = new TestableServiceBusProcessor<ModelingDataRejectedIntegrationEvent>();
                        TestableServiceBusProcessors.Add(testableProcessor);
                        return testableProcessor;
                    }

                    if (queue == $"{localMachineNamePrefix}{TestModelUpdatedEventQueueName}")
                    {
                        var testableProcessor = new TestableServiceBusProcessor<ModelUpdatedIntegrationEvent>();
                        TestableServiceBusProcessors.Add(testableProcessor);
                        return testableProcessor;
                    }

                    return null!;
                });
                services.AddSingleton(client.Object);

                if (SetSharedEventRepository is not null)
                    services.AddSingleton<IEventRepository>(_ => SetSharedEventRepository());

                //MockDbConnectionFactory.Setup(factory => factory.CreateConnection()).Returns(MockWrappedDbConnection.Object);
                //services.AddSingleton(provider => MockDbConnectionFactory.Object);

                //var mockTransaction = new Mock<IWrappedDbTransaction>();
                //MockWrappedDbConnection.Setup(connection => connection.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(mockTransaction.Object);

                //mockTransaction.Setup(transaction => transaction.GetConnection()).Returns(MockWrappedDbConnection.Object);
            });

        var host = base.CreateHost(builder);

        return host;
    }

    public TestableServiceBusProcessor<TEventType> GetTestableServiceBusProcessor<TEventType>()
        where TEventType : class => TestableServiceBusProcessors.Get<TestableServiceBusProcessor<TEventType>>();

    public void Start()
    {
        HttpClient = CreateClient();
    }
}
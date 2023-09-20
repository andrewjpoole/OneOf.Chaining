using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneOf.Chaining.Examples.Application.Exceptions;
using OneOf.Chaining.Examples.Application.Services;

namespace OneOf.Chaining.Examples.Infrastructure.MessageBus;

public class ServiceBusEventListener<T> : IHostedService, IDisposable
    where T : class
{
    private readonly ILogger<ServiceBusEventListener<T>> logger;
    private readonly IEventHandler<T> eventHandler;
    private readonly ServiceBusClient serviceBusClient;
    private ServiceBusProcessor? serviceBusProcessor;

    private readonly string queueName;
    private readonly int maxConcurrentCalls;
    private readonly int initialBackoffInMs;
    private readonly int maxJitterInMs;

    public ServiceBusEventListener(
        ServiceBusClient serviceBusClient,
        IOptions<ServiceBusOptions> queueHandlerOptions,
        IEventHandler<T> eventHandler,
        ILogger<ServiceBusEventListener<T>> logger)
    {
        this.logger = logger;
        this.serviceBusClient = serviceBusClient;
        this.eventHandler = eventHandler;
        initialBackoffInMs = queueHandlerOptions.Value.InitialBackoffInMs;
        maxConcurrentCalls = queueHandlerOptions.Value.MaxConcurrentCalls;
        maxJitterInMs = Convert.ToInt32(initialBackoffInMs * 0.1);
        queueName = new QueueOrTopicName(queueHandlerOptions.Value.ResolveQueueOrTopicNameFromConfig(typeof(T).Name)).Name;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogTrace($"Starting service, with MaxConcurrentCalls: {maxConcurrentCalls}");

            serviceBusProcessor = serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions
            {
                PrefetchCount = 1,
                AutoCompleteMessages = false,
                MaxConcurrentCalls = maxConcurrentCalls
            });

            serviceBusProcessor.ProcessMessageAsync += MessageHandler;
            serviceBusProcessor.ProcessErrorAsync += ErrorHandler;

            logger.LogDebug($"Starting to consume from Queue: {queueName}");

            await serviceBusProcessor.StartProcessingAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception thrown during startup");
            throw;
        }

        logger.LogTrace("Entering keep alive");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (serviceBusProcessor is null)
            return;

        await serviceBusProcessor.StopProcessingAsync(cancellationToken);
        await serviceBusProcessor.CloseAsync(cancellationToken);
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        // Add telemetry tracing code here

        try
        {
            var @event = args.Message.GetJsonPayload<T>();

            if (@event is null)
                throw new PermanentException("Unable to deserialize payload of ServiceBusReceivedMessage");

            await eventHandler.HandleEvent(@event);

            await args.CompleteMessageAsync(args.Message);
        }
        catch (JsonException e)
        {
            logger.LogError(
                $"JsonReaderException thrown while fetching payload before processing message, json payload is invalid, expected message body to be {typeof(T).Name}, message will be dead lettered: {e}");
            await args.DeadLetterMessageAsync(args.Message, e.Message);
        }
        catch (PermanentException e)
        {
            logger.LogError($"PermanentException thrown while processing message, message will be dead lettered: {e}");
            await args.DeadLetterMessageAsync(args.Message, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError($"Error thrown while processing message: {e}");

            // Abandon so the message can be retried after a sensible delay
            await Task.Delay(GetCappedExponentialBackoffDelay(args.Message.DeliveryCount));
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private int GetCappedExponentialBackoffDelay(int retryAttempt)
    {
        if (retryAttempt > 5)
            retryAttempt = 5;

        var jitterer = new Random();
        var exponential = Math.Pow(2, retryAttempt);
        var basicDelay = TimeSpan.FromMilliseconds(initialBackoffInMs * exponential);
        var jitteredDelay = basicDelay.Add(TimeSpan.FromMilliseconds(jitterer.Next(0, maxJitterInMs)));
        return (int)jitteredDelay.TotalMilliseconds;
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        logger.LogError($"Error handling message. {args.Exception}");

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        serviceBusProcessor?.DisposeAsync().ConfigureAwait(false);
    }
}
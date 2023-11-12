using System.Text.Json;
using Azure.Messaging.ServiceBus;
using OneOf.Chaining.Examples.Domain.EventSourcing;

namespace OneOf.Chaining.Examples.Tests.Framework;

public class TestableServiceBusProcessor<T> : ServiceBusProcessor where T : class
{
    public List<TestableProcessMessageEventArgs> MessageDeliveryAttempts = new();

    public async Task SendMessageWithRetries(T request, int maxDeliveryCount = 10)
    {
        for (var attempt = 1; attempt <= maxDeliveryCount; attempt++)
        {
            if (attempt > 1)
            {
                var previousAttempt = MessageDeliveryAttempts.Last();
                if (previousAttempt.WasDeadLettered || previousAttempt.WasCompleted)
                    return;
            }

            await SendMessage(request, attempt);
        }
    }

    public async Task SendMessage(T request, int deliveryCount = 1)
    {
        var args = CreateMessageArgs(request, deliveryCount);
        MessageDeliveryAttempts.Add((TestableProcessMessageEventArgs)args);
        await base.OnProcessMessageAsync(args);
    }

    public async Task SendMessage(string json)
    {
        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: BinaryData.FromString(json));

        var args = new TestableProcessMessageEventArgs(message);

        MessageDeliveryAttempts.Add(args);
        await base.OnProcessMessageAsync(args);
    }

    public override Task StartProcessingAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public static ProcessMessageEventArgs CreateMessageArgs(T payload, int deliveryCount = 1)
    {
        var payloadJson = JsonSerializer.Serialize(payload, GlobalJsonSerialiserSettings.Default);

        var correlationId = Guid.NewGuid().ToString();
        var applicationProperties = new Dictionary<string, object>
        {
            {"origin", "ComponentTests"}
        };

        var message = ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: BinaryData.FromString(payloadJson),
            correlationId: correlationId,
            properties: applicationProperties,
            deliveryCount: deliveryCount);

        var args = new TestableProcessMessageEventArgs(message);

        return args;
    }
}
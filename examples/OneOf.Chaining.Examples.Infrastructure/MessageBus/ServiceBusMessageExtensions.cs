using System.Text.Json;
using Azure.Messaging.ServiceBus;
using OneOf.Chaining.Examples.Domain.EventSourcing;

namespace OneOf.Chaining.Examples.Infrastructure.MessageBus;

public static class ServiceBusMessageExtensions
{
    public static T? GetJsonPayload<T>(this ServiceBusReceivedMessage message)
    {
        return JsonSerializer.Deserialize<T>(message.Body.ToString(), GlobalJsonSerialiserSettings.Default);
    }
}
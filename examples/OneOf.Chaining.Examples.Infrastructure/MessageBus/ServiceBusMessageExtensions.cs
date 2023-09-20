using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace OneOf.Chaining.Examples.Infrastructure.MessageBus;

public static class ServiceBusMessageExtensions
{
    public static T? GetJsonPayload<T>(this ServiceBusReceivedMessage message)
    {
        return JsonSerializer.Deserialize<T>(message.Body.ToString());
    }
}
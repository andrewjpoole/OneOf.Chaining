namespace OneOf.Chaining.Examples.Infrastructure.MessageBus;

public class ServiceBusOptions
{
    public Dictionary<string, string> Names { get; set; } = new();
    public int InitialBackoffInMs { get; set; } = 2000;
    public int MaxConcurrentCalls { get; set; }

    public string ResolveQueueOrTopicNameFromConfig(string messageClassName)
    {
        if (string.IsNullOrEmpty(messageClassName))
            throw new ArgumentNullException(nameof(messageClassName));

        var queueOrTopicExists = Names.TryGetValue(messageClassName, out var queueOrTopicName);

        if (!queueOrTopicExists)
            throw new Exception($"Can't find a queue or topic in config for the key {messageClassName}");

        if (string.IsNullOrWhiteSpace(queueOrTopicName))
            throw new Exception($"Found the key {messageClassName} in the Name dictionary, but no value is provided containing the name of the queue or topic");

        return queueOrTopicName; // this should contain the queue or topic name found in the config file, bound by the options pattern.
    }
}

public class ServiceBusInboundQueueHandlerOptions : ServiceBusOptions
{
    public const string Name = "ServiceBus:Inbound";
}

public class ServiceBusOutboundEntitiyOptions : ServiceBusOptions
{
    public const string Name = "ServiceBus:Outbound";
}
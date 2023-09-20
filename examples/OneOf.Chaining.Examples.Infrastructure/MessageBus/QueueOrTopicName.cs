namespace OneOf.Chaining.Examples.Infrastructure.MessageBus;

public class QueueOrTopicName
{
    public string Name { get; }
    public QueueOrTopicName(string queueOrTopicName)
    {
#if DEBUG
        Name = $"{Environment.MachineName}-{queueOrTopicName}";
#else
        Name = queueOrTopicName;
#endif
    }
}
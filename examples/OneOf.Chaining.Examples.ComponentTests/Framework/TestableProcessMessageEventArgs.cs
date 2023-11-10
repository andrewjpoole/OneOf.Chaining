using Azure.Messaging.ServiceBus;

namespace OneOf.Chaining.Examples.Tests.Framework;

public class TestableProcessMessageEventArgs : ProcessMessageEventArgs
{
    public bool WasCompleted;
    public bool WasDeadLettered;
    public bool WasAbandoned;
    public DateTime Created;
    public string DeadLetterReason = string.Empty;

    public TestableProcessMessageEventArgs(ServiceBusReceivedMessage message) : base(message, null, CancellationToken.None)
    {
        Created = DateTime.UtcNow;
    }

    public override Task CompleteMessageAsync(ServiceBusReceivedMessage message,
        CancellationToken cancellationToken = new())
    {
        WasCompleted = true;
        return Task.CompletedTask;
    }

    public override Task DeadLetterMessageAsync(ServiceBusReceivedMessage message, string deadLetterReason,
        string? deadLetterErrorDescription = null, CancellationToken cancellationToken = new())
    {
        WasDeadLettered = true;
        DeadLetterReason = deadLetterReason;
        return Task.CompletedTask;
    }

    public override Task AbandonMessageAsync(ServiceBusReceivedMessage message, IDictionary<string, object>? propertiesToModify = null,
        CancellationToken cancellationToken = new())
    {
        WasAbandoned = true;
        return Task.CompletedTask;
    }
}
using System.Text.Json;

namespace OneOf.Chaining.Examples.Domain.EventSourcing;

public class Event
{
    protected Event(Guid requestId, string eventClassName, string serialisedEvent)
    {
        RequestId = requestId;
        EventClassName = eventClassName;
        SerialisedEvent = serialisedEvent;
    }

    public Guid RequestId { get; protected set; }
    public Guid LocationId { get; protected set; }
    public object? Value { get; protected set; }
    public string EventClassName { get; protected set; }
    public string SerialisedEvent { get; protected set; }

    public static Event Create<T>(T value, Guid requestId) where T : IDomainEvent
    {
        var @event = new Event(requestId, typeof(T).FullName!, JsonSerializer.Serialize(value, GlobalJsonSerialiserSettings.Default));
        @event.Value = value;
        return @event;
    }
}
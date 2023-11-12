using System.Net.NetworkInformation;
using System.Text.Json;

namespace OneOf.Chaining.Examples.Domain.EventSourcing;

public class PersistedEvent : Event
{
    public PersistedEvent(long id, Guid groupingId, string eventClassName, string serialisedEvent, DateTime timestampCreatedUtc)
        : base(groupingId, eventClassName, serialisedEvent)
    {
        Id = id;
        TimestampCreatedUTC = timestampCreatedUtc;
    }

    public long Id { get; }
    public DateTime TimestampCreatedUTC { get; }

    public T To<T>()
    {
        if (Value == null)
        {
            var value = JsonSerializer.Deserialize<T>(SerialisedEvent, GlobalJsonSerialiserSettings.Default);

            if (value == null)
                throw new Exception($"SerialisedEvent could not be de-serialised into {typeof(T).FullName}.");

            Value = value;
        }

        return (T)Value;
    }

    public override string ToString() => EventClassName;
}
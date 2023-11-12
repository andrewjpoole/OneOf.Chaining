namespace OneOf.Chaining.Examples.Domain.EventSourcing;

public static class PersistedEventsExtensions
{
    public static T? To<T>(this List<PersistedEvent> persistedEvents)
    {
        var eventClassName = typeof(T).FullName;
        var selectedPersistedEvent = persistedEvents.LastOrDefault(pe => pe.EventClassName == eventClassName);

        var eventNotYetHappened = default(T);

        return selectedPersistedEvent == null ? eventNotYetHappened : selectedPersistedEvent.To<T>();
    }
}
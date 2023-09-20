namespace OneOf.Chaining.Examples.Application.Services;

public interface IEventHandler<in T> where T : class
{
    Task HandleEvent(T @event);
}

public interface IEventHandlerDataAccepted<in T> : IEventHandler<T> where T : class
{
    Task HandleDataAcceptedEvent(T @event);
}
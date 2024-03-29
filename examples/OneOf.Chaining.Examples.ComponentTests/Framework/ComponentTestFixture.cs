using OneOf.Chaining.Examples.Infrastructure.Persistence;

namespace OneOf.Chaining.Examples.Tests.Framework;

public class ComponentTestFixture : IDisposable
{
    private string phase = "";

    public readonly ApiWebApplicationFactory ApiFactory;
    public readonly EventListenerWebApplicationFactory EventListenerFactory;

    public EventRepository EventRepository = new();

    public ComponentTestFixture()
    {
        ApiFactory = new ApiWebApplicationFactory();
        ApiFactory.SetSharedEventRepository = () => EventRepository;

        EventListenerFactory = new EventListenerWebApplicationFactory();
        EventListenerFactory.SetSharedEventRepository = () => EventRepository;
    }

    public void Dispose()
    {
        ApiFactory.HttpClient?.Dispose();
        EventListenerFactory.HttpClient?.Dispose();
    }

    public (Given given, When when, Then then) SetupHelpers()
    {
        return (new Given(this), new When(this), new Then(this));
    }

    public void SetPhase(string newPhase) => phase = newPhase;
    public string CurrentPhase => string.IsNullOrWhiteSpace(phase) ? string.Empty : $"In phase {phase}, ";
}
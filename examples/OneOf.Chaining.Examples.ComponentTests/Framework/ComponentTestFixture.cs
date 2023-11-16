using OneOf.Chaining.Examples.Infrastructure.Persistence;

namespace OneOf.Chaining.Examples.Tests.Framework;

public class ComponentTestFixture : IDisposable
{
    private string phase = "";

    public readonly ApiWebApplicationFactory ApiFactory;
    public readonly EventListenerWebApplicationFactory EventListenerFactory;

    public EventRepository RealSharedEventRepository = new();

    public ComponentTestFixture()
    {
        ApiFactory = new ApiWebApplicationFactory();
        ApiFactory.SetSharedEventRepository = () => RealSharedEventRepository;

        EventListenerFactory = new EventListenerWebApplicationFactory();
        EventListenerFactory.SetSharedEventRepository = () => RealSharedEventRepository;
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

    public void SetPhase(string newPhase) => this.phase = newPhase;
    public string CurrentPhase => string.IsNullOrWhiteSpace(phase) ? string.Empty : $"In phase {phase}, ";
}
using OneOf.Chaining.Examples.Infrastructure.Persistence;

namespace OneOf.Chaining.Examples.Tests.Framework;

public class ComponentTestFixture : IDisposable
{
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
        ApiFactory.Dispose();

        EventListenerFactory.HttpClient?.Dispose();
        EventListenerFactory.Dispose();
    }

    public (Given given, When when, Then then) SetupHelpers()
    {
        return (new Given(this), new When(this), new Then(this));
    }
}
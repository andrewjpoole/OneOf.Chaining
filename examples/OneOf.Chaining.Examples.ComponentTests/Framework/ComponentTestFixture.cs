using OneOf.Chaining.Examples.Infrastructure.Persistence;

namespace OneOf.Chaining.Examples.Tests.Framework;

public class ComponentTestFixture : IDisposable
{
    public readonly ApiWebApplicationFactory ApiFactory;
    public readonly EventListenerWebApplicationFactory EventListenerFactory;

    public WeatherDataPersistence RealWeatherDataPersistence = new();

    public ComponentTestFixture()
    {
        ApiFactory = new ApiWebApplicationFactory();
        ApiFactory.SetSharedPersistence = () => RealWeatherDataPersistence;

        EventListenerFactory = new EventListenerWebApplicationFactory();
        EventListenerFactory.SetSharedPersistence = () => RealWeatherDataPersistence;
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
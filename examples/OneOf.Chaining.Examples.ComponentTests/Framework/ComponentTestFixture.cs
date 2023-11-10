namespace OneOf.Chaining.Examples.Tests.Framework;

public class ComponentTestFixture : IDisposable
{
    public readonly ApiWebApplicationFactory ApiFactory;


    public ComponentTestFixture()
    {
        ApiFactory = new ApiWebApplicationFactory();
    }

    public void Dispose()
    {
        ApiFactory.Dispose();
        ApiFactory.HttpClient?.Dispose();
    }

    public (Given given, When when, Then then) SetupHelpers()
    {
        return (new Given(this), new When(this), new Then(this));
    }
}
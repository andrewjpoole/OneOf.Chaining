using Microsoft.AspNetCore.Mvc.Testing;

namespace OneOf.Chaining.Examples.Tests.Framework;

public class EventListenerWebApplicationFactory : WebApplicationFactory<Program>
{
    public HttpClient? HttpClient;



    public void Start()
    {
        HttpClient = CreateClient();
    }
}
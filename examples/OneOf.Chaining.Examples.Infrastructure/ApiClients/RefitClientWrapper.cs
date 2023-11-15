using System.Text.Json;
using Refit;

namespace OneOf.Chaining.Examples.Infrastructure.ApiClients;

public class RefitClientWrapper<T>(IHttpClientFactory clientFactory) : IRefitClientWrapper<T>
{
    private readonly IHttpClientFactory clientFactory = clientFactory;

    public T CreateClient()
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var typeOfT = typeof(T);
        var nameofT = typeOfT.FullName ?? typeOfT.Name;
        var refitClient = RestService.For<T>(clientFactory.CreateClient(nameofT), new RefitSettings(new SystemTextJsonContentSerializer(jsonSerializerOptions)));

        return refitClient;
    }
}
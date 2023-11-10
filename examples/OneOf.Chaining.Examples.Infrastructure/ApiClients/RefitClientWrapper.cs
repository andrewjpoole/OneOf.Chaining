using System.Text.Json;
using Refit;

public class RefitClientWrapper<T> : IRefitClientWrapper<T>
{
    private readonly IHttpClientFactory clientFactory;

    public RefitClientWrapper(IHttpClientFactory clientFactory)
    {
        this.clientFactory = clientFactory;
    }

    public T CreateClient()
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var nameofT = typeof(T).FullName;
        var refitClient = RestService.For<T>(clientFactory.CreateClient(nameofT), new RefitSettings(new SystemTextJsonContentSerializer(jsonSerializerOptions)));

        return refitClient;
    }
}
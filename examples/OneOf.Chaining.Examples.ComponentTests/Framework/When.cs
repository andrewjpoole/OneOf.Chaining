using System.Text;
using System.Text.Json;
using OneOf.Chaining.Examples.Application.Models.Events.WeatherModelingEvents;
using OneOf.Chaining.Examples.Application.Models.Requests;

namespace OneOf.Chaining.Examples.Tests.Framework;

public class When
{
    private readonly ComponentTestFixture fixture;


    public When(ComponentTestFixture fixture)
    {
        this.fixture = fixture;
    }

    public static When UsingThe(ComponentTestFixture fixture) => new(fixture);
    public When And => this;

    public When WeSendTheMessageToTheApi(HttpRequestMessage httpRequest, out HttpResponseMessage response)
    {
        if (fixture.ApiFactory.HttpClient is null)
            throw new Exception("http client has not been initialised");

        response = fixture.ApiFactory.HttpClient.SendAsync(httpRequest).GetAwaiter().GetResult();

        if(response.IsSuccessStatusCode)
            return this;

        var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        throw new Exception($"Test Request from When.WeSendTheMessageToTheApi() was not successful; {body}");
    }

    public When WeWrapTheCollectedWeatherDataInAnHttpRequestMessage(CollectedWeatherDataModel collectedWeatherDataModel, string location, out HttpRequestMessage httpRequest)
    {
        httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{Constants.WeatherModelingServiceSubmissionUri}{location}");
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(collectedWeatherDataModel), Encoding.UTF8, "application/json");

        return this;
    }

    public When AMessageAppears(ModelingEvent message)
    {
        // todo: present message to EventListener's appropriate TestableServiceBusListener

        return this;
    }
}
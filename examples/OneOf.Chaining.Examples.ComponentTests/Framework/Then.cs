using System.Net;
using System.Text.Json;
using FluentAssertions;
using Moq;
using Moq.Contrib.HttpClient;
using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain;

namespace OneOf.Chaining.Examples.Tests.Framework;

public class Then
{
    private readonly ComponentTestFixture fixture;

    public Then(ComponentTestFixture fixture)
    {
        this.fixture = fixture;
    }

    public Then And => this;

    public Then AndAssert(Action assertion)
    {
        assertion();
        return this;
    }

    public Then TheResponseCodeShouldBe(HttpResponseMessage response, HttpStatusCode code)
    {
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(code);
        return this;
    }

    public Then TheBodyShouldBeEmpty(HttpResponseMessage response)
    {
        var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        body.Should().BeEmpty();

        return this;
    }

    public Then TheBodyShouldNotBeEmpty(HttpResponseMessage response, out string body)
    {
        body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        body.Should().NotBeEmpty();

        return this;
    }

    public Then TheBodyShouldNotBeEmpty(HttpResponseMessage response, Action<string>? assertAgainstBody = null)
    {
        TheBodyShouldNotBeEmpty(response, out var body);

        assertAgainstBody?.Invoke(body);

        return this;
    }

    public Then TheBodyShouldNotBeEmpty<T>(HttpResponseMessage response, Action<T>? assertAgainstBody = null)
    {
        TheBodyShouldNotBeEmpty(response, out var body);

        var bodyT = JsonSerializer.Deserialize<T>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (bodyT is null)
            throw new Exception($"unable to deserialise body into {nameof(T)}. Body:{body}");

        assertAgainstBody?.Invoke(bodyT);

        return this;
    }
    
    public Then TheModelingServiceSubmitEndpointShouldHaveBeenCalled(int times = 1)
    {
        fixture.ApiFactory.MockWeatherModelingServiceHttpMessageHandler
            .VerifyRequest(HttpMethod.Post, r => r.RequestUri!.ToString().StartsWith($"{Constants.WeatherModelingServiceBaseUrl}{Constants.WeatherModelingServiceSubmissionUri}"), Times.Exactly(times));

        return this;
    }

    public Then TheEventShouldHaveBeenPersisted(EventNames eventName)
    {
        fixture.RealWeatherDataPersistence?.CollectedWeatherDataStatusUpdateRepository.Should().Contain(update => update.EventName == eventName.ToString());

        return this;
    }
}
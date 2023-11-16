using System.Net;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Moq;
using Moq.Contrib.HttpClient;
using OneOf.Chaining.Examples.Application.Models.Requests;
using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain;
using OneOf.Chaining.Examples.Domain.EventSourcing;

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
        response.StatusCode.Should().Be(code, $"In {fixture.CurrentPhase}, expected that the response code was {code}.");
        return this;
    }

    public Then TheBodyShouldBeEmpty(HttpResponseMessage response)
    {
        var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        body.Should().BeEmpty($"{fixture.CurrentPhase}expected that the response body would be empty.");

        return this;
    }

    public Then TheBodyShouldNotBeEmpty(HttpResponseMessage response, out string body)
    {
        body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        body.Should().NotBeEmpty($"{fixture.CurrentPhase}expected that the response body would not be empty.");

        return this;
    }

    public Then TheBodyShouldNotBeEmpty<T>(HttpResponseMessage response, out T bodyAsT)
    {
        TheBodyShouldNotBeEmpty(response, out var body);
        T bodyT;
        try
        {
            bodyT = JsonSerializer.Deserialize<T>(body, GlobalJsonSerialiserSettings.Default) ?? throw new Exception();
        }
        catch (Exception e)
        {
            var typeOfT = typeof(T);
            throw new Exception($"{fixture.CurrentPhase}Then.TheBodyShouldNotBeEmpty<{typeOfT.Name}>(). Unable to deserialise response body into {typeOfT.Name}. Body:{body}", e);
        }

        bodyAsT = bodyT;

        return this;
    }

    public Then TheBodyShouldNotBeEmpty<T>(HttpResponseMessage response, Action<T>? assertAgainstBody = null)
    {
        TheBodyShouldNotBeEmpty<T>(response, out var bodyT);
        
        assertAgainstBody?.Invoke(bodyT);

        return this;
    }

    public Then TheModelingServiceSubmitEndpointShouldHaveBeenCalled(int times = 1)
    {
        fixture.ApiFactory.MockWeatherModelingServiceHttpMessageHandler
            .VerifyRequest(HttpMethod.Post, 
                r => r.RequestUri!.ToString().StartsWith($"{Constants.WeatherModelingServiceBaseUrl}{Constants.WeatherModelingServiceSubmissionUri}"), 
                Times.Exactly(times), $"{fixture.CurrentPhase}expected the ModelingServiceSubmitEndpoint to have been called {times} time(s).");

        return this;
    }

    public Then TheEventShouldHaveBeenPersisted<T>()
    {
        var typeOfT = typeof(T);
        var eventClassName = typeOfT.FullName ?? typeOfT.Name;
        fixture.RealSharedEventRepository?.PersistedEvents.Should().Contain(e => e.EventClassName == eventClassName,
            $"{fixture.CurrentPhase}expected an event of type {eventClassName} to have been persisted in the database.");

        return this;
    }
}
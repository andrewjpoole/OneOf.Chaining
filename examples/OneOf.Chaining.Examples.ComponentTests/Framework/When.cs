using System.Text;
using System.Text.Json;
using OneOf.Chaining.Examples.Application.Models.Requests;
using OneOf.Chaining.Examples.Domain.EventSourcing;

namespace OneOf.Chaining.Examples.Tests.Framework;

public class When
{
    private readonly ComponentTestFixture fixture;


    public When(ComponentTestFixture fixture)
    {
        this.fixture = fixture;
    }
    public When And => this;

    public When InPhase(string newPhase)
    {
        fixture.SetPhase(newPhase);
        return this;
    }

    public When WeSendTheMessageToTheApi(HttpRequestMessage httpRequest, out HttpResponseMessage response)
    {
        if (fixture.ApiFactory.HttpClient is null)
            throw new Exception("The Http client has not been initialised, please ensure Given.TheServerHasStarted() has been called");

        response = fixture.ApiFactory.HttpClient.SendAsync(httpRequest).GetAwaiter().GetResult();

        if(response.IsSuccessStatusCode)
            return this;

        var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        throw new Exception($"Test Request from When.WeSendTheMessageToTheApi() in phase {fixture.CurrentPhase} was not successful; {body}");
    }

    public When WeWrapTheCollectedWeatherDataInAnHttpRequestMessage(CollectedWeatherDataModel collectedWeatherDataModel, string location, out HttpRequestMessage httpRequest)
    {
        httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{Constants.WeatherModelingServiceSubmissionUri}{location}");
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(collectedWeatherDataModel, GlobalJsonSerialiserSettings.Default), Encoding.UTF8, "application/json");

        return this;
    }

    public When AMessageAppears<T>(T message) where T : class
    {
        var processor = fixture.EventListenerFactory.GetTestableServiceBusProcessor<T>();
        processor.SendMessage(message).GetAwaiter().GetResult();

        return this;
    }
}
using System;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using OneOf.Chaining.Examples.Application.Models;
using OneOf.Chaining.Examples.Application.Models.Events.WeatherModelingEvents;
using OneOf.Chaining.Examples.Application.Models.Requests;
using OneOf.Chaining.Examples.Application.Services;
using OneOf.Chaining.Examples.Domain;
using OneOf.Chaining.Examples.Tests.Framework;

namespace OneOf.Chaining.Examples.Tests;

[Collection(nameof(NonParallelCollectionDefinition))]
public class ComponentTests : IClassFixture<ComponentTestFixture>
{
    private readonly ComponentTestFixture testFixture;

    public ComponentTests(ComponentTestFixture testFixture)
    {
        this.testFixture = testFixture;
    }

    [Fact]
    public void Return_a_WeatherReport_given_valid_region_and_date()
    {
        Given.UsingThe(testFixture)
            .WeHaveAWeatherReportRequest("bristol", DateTime.Now, out var apiRequest)
            .And.TheServerIsStarted();

        When.UsingThe(testFixture)
            .WeSendTheMessageToTheApi(apiRequest, out var response);

        Then.UsingThe(testFixture)
            .TheResponseCodeShouldBe(response, HttpStatusCode.OK)
            .And.TheBodyShouldNotBeEmpty<WeatherReportResponse>(response, x => x.Summary.Should().NotBeEmpty());
    }

    [Fact]
    public void Return_accepted_for_valid_request()
    {
        var (given, when, then) = testFixture.SetupHelpers();

        given
            .WeHaveSomeCollectedWeatherData(out var collectedWeatherModel)
            .And.TheModelingServiceSubmitEndpointWillReturn(HttpStatusCode.Accepted, out var submissionId)
            .And.TheServerIsStarted();
        
        when // in phase 1
            .And.WeWrapTheCollectedWeatherDataInAnHttpRequestMessage(collectedWeatherModel, "testLocation", out var httpRequest)
            .And.WeSendTheMessageToTheApi(httpRequest, out var response);

        then
            .And.TheModelingServiceSubmitEndpointShouldHaveBeenCalled(times: 1)
            .And.TheEventShouldHaveBeenPersisted(EventNames.SubmittedToModeling)
            .And.TheResponseCodeShouldBe(response, HttpStatusCode.OK)
            .And.TheBodyShouldNotBeEmpty<WeatherDataCollectionResponse>(response, x =>x.RequestId.Should().NotBeEmpty());
        
        when // in phase 2
            .AMessageAppears(message: new ModelingDataAcceptedEvent(submissionId));

        //then
        //    .TheEventShouldHaveBeenPersisted(EventNames.ModelingDataAccepted);

        //when // in phase 3
        //    .AMessageAppears(message: new ModelUpdatedEvent(submissionId));

        //then
        //    .TheEventShouldHaveBeenPersisted(EventNames.ModelUpdated);

    }
}
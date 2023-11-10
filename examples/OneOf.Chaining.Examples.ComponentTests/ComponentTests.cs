using System.Net;
using FluentAssertions;
using OneOf.Chaining.Examples.Application.Models;
using OneOf.Chaining.Examples.Application.Models.Events.WeatherModelingEvents;
using OneOf.Chaining.Examples.Application.Models.Requests;
using OneOf.Chaining.Examples.Application.Services;
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
        var (given, when, then) = testFixture.SetupHelpers();

        given.WeHaveAWeatherReportRequest("bristol", DateTime.Now, out var apiRequest)
            .And.TheServerIsStarted();

        when.WeSendTheMessageToTheApi(apiRequest, out var response);

        then.TheResponseCodeShouldBe(response, HttpStatusCode.OK)
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
        
        when // in phase 1 (initial API request)
            .And.WeWrapTheCollectedWeatherDataInAnHttpRequestMessage(collectedWeatherModel, "testLocation", out var httpRequest)
            .And.WeSendTheMessageToTheApi(httpRequest, out var response);

        then
            .And.TheModelingServiceSubmitEndpointShouldHaveBeenCalled(times: 1)
            .And.TheEventShouldHaveBeenPersisted(EventNames.SubmittedToModeling)
            .And.TheResponseCodeShouldBe(response, HttpStatusCode.OK)
            .And.TheBodyShouldNotBeEmpty<WeatherDataCollectionResponse>(response, x =>x.RequestId.Should().NotBeEmpty());
        
        when // in phase 2 (1st ASB message back from modeling service)
            .AMessageAppears(message: new ModelingDataAcceptedEvent(submissionId));

        then
            .TheEventShouldHaveBeenPersisted(EventNames.ModelingDataAccepted);

        //when // in phase 3 (2nd ASB message back from modeling service)
        //    .AMessageAppears(message: new ModelUpdatedEvent(submissionId));

        //then
        //    .TheEventShouldHaveBeenPersisted(EventNames.ModelUpdated);

    }
}
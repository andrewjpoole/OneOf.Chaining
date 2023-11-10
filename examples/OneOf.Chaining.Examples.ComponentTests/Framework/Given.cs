using System.Net;
using Moq.Contrib.HttpClient;
using OneOf.Chaining.Examples.Application.Models.Requests;
using OneOf.Chaining.Examples.Domain.Entities;

namespace OneOf.Chaining.Examples.Tests.Framework;

public class Given
{
    private readonly ComponentTestFixture fixture;
    private readonly Random random = new();

    public Given(ComponentTestFixture fixture)
    {
        this.fixture = fixture;
    }
    public Given And => this;

    public Given WeHaveAWeatherReportRequest(string region, DateTime date, out HttpRequestMessage request)
    {
        request = new HttpRequestMessage(HttpMethod.Get, $"v1/weather-forecast/{region}/{date:s}");
        return this;
    }

    public Given WeHaveSomeCollectedWeatherData(out CollectedWeatherDataModel data)
    {
        data = new CollectedWeatherDataModel(new List<CollectedWeatherDataPointModel>
        {
            CannedData.GetRandCollectedWeatherDataModel(),
            CannedData.GetRandCollectedWeatherDataModel(),
            CannedData.GetRandCollectedWeatherDataModel()
        });

        return this;
    }

    public Given TheServerIsStarted()
    {
        fixture.ApiFactory.Start();
        fixture.EventListenerFactory.Start();
        return this;
    }

    public Given TheModelingServiceSubmitEndpointWillReturn(HttpStatusCode statusCode, out Guid submissionId)
    {
        submissionId = Guid.NewGuid();
        fixture.ApiFactory.MockWeatherModelingServiceHttpMessageHandler
            .SetupRequest(HttpMethod.Post, r => r.RequestUri!.ToString().StartsWith($"{Constants.WeatherModelingServiceBaseUrl}{Constants.WeatherModelingServiceSubmissionUri}"))
            .ReturnsResponse(statusCode, new StringContent(submissionId.ToString()));
        return this;
    }
}
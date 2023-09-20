using Microsoft.AspNetCore.Mvc;
using OneOf.Chaining.Examples.Domain.Outcomes;

namespace OneOf.Chaining.Examples.WebAPI;

public static class FailureExtensions
{
    public static ProblemDetails ToProblemDetails(this UnsupportedRegionFailure failure)
    {
        return new ProblemDetails
        {
            Type = "some origin...",
            Title = failure.Title,
            Detail = failure.Detail
        };
    }
}
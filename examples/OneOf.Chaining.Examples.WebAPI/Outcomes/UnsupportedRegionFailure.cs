using Microsoft.AspNetCore.Mvc;

public class UnsupportedRegionFailure
{
    public string Title { get; set; }

    public string Detail { get; set; }

    public UnsupportedRegionFailure(string title, string detail)
    {
        Title = title;
        Detail = detail;
    }

    public ProblemDetails ToProblemDetails()
    {
        return new ProblemDetails
        {
            Type = "some origin...",
            Title = Title,
            Detail = Detail
        };
    }
}
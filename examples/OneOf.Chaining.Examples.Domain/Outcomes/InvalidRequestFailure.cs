namespace OneOf.Chaining.Examples.Domain.Outcomes;

public class InvalidRequestFailure
{
    public IDictionary<string, string[]> ValidationErrors { get; }

    public InvalidRequestFailure(IDictionary<string, string[]> validationErrors)
    {
        ValidationErrors = validationErrors;
    }

    public InvalidRequestFailure(string message)
    {
        ValidationErrors = new Dictionary<string, string[]>{{"model", new[] {message}}};
    }
}
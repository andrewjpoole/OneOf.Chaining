public class InvalidRequest
{
    public IDictionary<string, string[]> ValidationErrors { get; }

    public InvalidRequest(IDictionary<string, string[]> validationErrors)
    {
        ValidationErrors = validationErrors;
    }
}
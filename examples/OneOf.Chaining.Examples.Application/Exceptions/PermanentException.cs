namespace OneOf.Chaining.Examples.Application.Exceptions;

public class PermanentException : Exception
{
    public PermanentException(string message) : base(message)
    {
    }
}
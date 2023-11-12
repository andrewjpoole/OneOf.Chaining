using OneOf.Chaining.Examples.Domain.BusinessRules;

namespace OneOf.Chaining.Examples.Domain.ValueObjects;

public record WindDirection
{
    public string Value { get; }

    public WindDirection(string value)
    {
        var windDirection = value;
        Rules.StringRules.CheckNotNullEmptyOrWhitespace(windDirection);
        Rules.StringRules.CheckMaxLength(windDirection, 5);

        // todo accept decimal degrees or "SSW" etc?

        Value = value;
    }
}
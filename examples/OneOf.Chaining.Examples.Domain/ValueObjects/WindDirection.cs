using OneOf.Chaining.Examples.Domain.BusinessRules;

namespace OneOf.Chaining.Examples.Domain.ValueObjects;

public record WindDirection
{
    public string Value { get; }

    public WindDirection(string windDirection)
    {
        Rules.StringRules.CheckNotNullEmptyOrWhitespace(windDirection, () => windDirection);
        Rules.StringRules.CheckMaxLength(windDirection, () => windDirection, 5);

        // todo accept decimal degrees or "SSW" etc?

        Value = windDirection;
    }
}
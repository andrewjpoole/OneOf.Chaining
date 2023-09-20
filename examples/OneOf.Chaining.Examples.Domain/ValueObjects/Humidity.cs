using OneOf.Chaining.Examples.Domain.BusinessRules;

namespace OneOf.Chaining.Examples.Domain.ValueObjects;

public record Humidity
{
    public decimal Value { get; }

    public string Unit => "%";

    public Humidity(decimal humidity)
    {
        Rules.DecimalRules.CheckPositive(humidity, () => humidity);
        Rules.DecimalRules.CheckIsWithinRange(humidity, () => humidity, Unit, 100M);

        Value = humidity;
    }
}
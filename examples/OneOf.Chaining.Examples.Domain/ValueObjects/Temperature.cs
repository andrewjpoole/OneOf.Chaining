using OneOf.Chaining.Examples.Domain.BusinessRules;

namespace OneOf.Chaining.Examples.Domain.ValueObjects;

public record Temperature
{
    public decimal Value { get; }

    public string Unit => "°C";

    public Temperature(decimal temperature)
    {
        Rules.DecimalRules.CheckIsWithinRange(temperature, () => temperature, Unit, -30.0M, 50.0M);

        Value = temperature;
    }
}
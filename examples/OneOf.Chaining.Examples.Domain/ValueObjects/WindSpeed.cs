using OneOf.Chaining.Examples.Domain.BusinessRules;

namespace OneOf.Chaining.Examples.Domain.ValueObjects;

public record WindSpeed
{
    public decimal Value { get; }

    public string Unit => "m/s";

    public WindSpeed(decimal windspeed)
    {
        Rules.DecimalRules.CheckPositive(windspeed, () => windspeed);
        Rules.DecimalRules.CheckIsWithinRange(windspeed, () => windspeed, Unit, 70M);

        Value = windspeed;
    }
}
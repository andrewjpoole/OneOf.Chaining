using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using OneOf.Chaining.Examples.Domain.Exceptions;

namespace OneOf.Chaining.Examples.Domain.BusinessRules;

public static class Rules
{
    public static class DecimalRules
    {
        public static void CheckPositive(
            decimal valueObject, 
            [CallerArgumentExpression(parameterName: "valueObject")] string? paramName = null)
        {
            if(valueObject < 0)
                throw new DomainValidationException($"{paramName} must be positive.");
        }

        public static void CheckIsWithinRange(
            decimal valueObject, 
            string unit, 
            decimal max, 
            decimal? min = null,
            [CallerArgumentExpression(parameterName: "valueObject")] string? paramName = null)
        {
            if (valueObject > max)
                throw new DomainValidationException($"{paramName} is out of range, must less than {max} {unit}.");

            if (min is not null && valueObject < min)
                throw new DomainValidationException($"{paramName} is out of range, must greater than {min} {unit}.");
        }
    }

    public static class StringRules
    {
        public static void CheckMaxLength(
            string valueObject,
            int maxLength,
            [CallerArgumentExpression(parameterName: "valueObject")] string? paramName = null)
        {
            if (valueObject.Length > maxLength)
                throw new DomainValidationException($"{paramName} exceeds max length of {maxLength}.");
        }

        public static void CheckNotNullEmptyOrWhitespace(
            [NotNull] string valueObject,
            [CallerArgumentExpression(parameterName: "valueObject")] string? paramName = null)
        {
            if (string.IsNullOrWhiteSpace(valueObject))
                throw new DomainValidationException($"{paramName} must not be null, empty or whitespace.");
        }
    }

    public static class GuidRules
    {
        public static void CheckNotEmpty(
            Guid valueObject,
            [CallerArgumentExpression(parameterName: "valueObject")] string? paramName = null)
        {
            if (valueObject == Guid.Empty)
                throw new DomainValidationException($"{paramName} must not be an empty Guid.");
        }
    }

    public static class JsonRules
    {
        public static void CheckValid([NotNull] string json,
            [CallerArgumentExpression(parameterName: "json")] string? paramName = null)
        {
            if (string.IsNullOrEmpty(json))
                throw new JsonException($"{paramName} is null or empty");

            JsonDocument.Parse(json); // throws JsonException if string is not valid json
        }
    }
}
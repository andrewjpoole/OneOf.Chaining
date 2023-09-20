using System.Linq.Expressions;
using OneOf.Chaining.Examples.Domain.Exceptions;

namespace OneOf.Chaining.Examples.Domain.BusinessRules;

public static class Rules
{
    public static class StringRules
    {
        public static void CheckMaxLength(string valueObject, Expression<Func<object>> expForPropertyName, int maxLength)
        {
            if (valueObject.Length > maxLength)
                throw new DomainValidationException($"{GetName(expForPropertyName)} exceeds max length of {maxLength}.");
        }

        public static void CheckNotNullEmptyOrWhitespace(string valueObject, Expression<Func<object>> expForPropertyName)
        {
            if (string.IsNullOrWhiteSpace(valueObject))
                throw new DomainValidationException($"{GetName(expForPropertyName)} must not be null, empty or whitespace.");
        }
    }

    public static class DecimalRules
    {
        public static void CheckPositive(decimal valueObject, Expression<Func<object>> expForPropertyName)
        {
            if(valueObject < 0)
                throw new DomainValidationException($"{GetName(expForPropertyName)} must be positive.");
        }

        public static void CheckIsWithinRange(decimal valueObject, Expression<Func<object>> expForPropertyName, string unit, decimal max, decimal? min = null)
        {
            if (valueObject > max)
                throw new DomainValidationException($"{GetName(expForPropertyName)} is out of range, must less than {max} {unit}.");

            if (min is not null && valueObject < min)
                throw new DomainValidationException($"{GetName(expForPropertyName)} is out of range, must greater than {min} {unit}.");
        }
    }

    public static string GetName(Expression<Func<object>> exp)
    {
        var body = exp.Body as MemberExpression;

        if (body is null)
        {
            var unaryExpressionBody = (UnaryExpression)exp.Body;
            body = unaryExpressionBody.Operand as MemberExpression;
        }

        if (body is null)
            throw new Exception("Expression must be a MemberExpression or a UnaryExpression");

        return body.Member.Name;
    }
}
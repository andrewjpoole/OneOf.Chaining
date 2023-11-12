using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace OneOf.Chaining.Examples.Domain.BusinessRules;

public static class Check
{
    public static void NotNull<T>(
        [NotNull] T? value,
        [CallerArgumentExpression(parameterName: "value")] string? paramName = null)
    {
        if (value == null)
            throw new ArgumentNullException(paramName);
    }
}
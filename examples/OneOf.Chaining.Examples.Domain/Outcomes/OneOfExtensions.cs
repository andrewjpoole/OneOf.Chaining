namespace OneOf.Chaining.Examples.Domain.Outcomes;

public static class OneOfExtensions
{
    public static bool Is<T>(this IOneOf oneOf) =>
        oneOf.Value.GetType() == typeof(T);

    public static T As<T>(this IOneOf oneOf) =>
        oneOf.Is<T>() ? (T)oneOf.Value : throw new Exception("object is not of type T.");

}
using System.Text.Json;

namespace OneOf.Chaining.Examples.Domain.EventSourcing;

public static class GlobalJsonSerialiserSettings
{
    public static JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true, 
        IncludeFields = true
    };
}
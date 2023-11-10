using OneOf.Chaining.Examples.EventListener.Extensions;

namespace OneOf.Chaining.Examples.EventListener;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.ConfigureServices();

        var app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        await app.RunAsync();
    }
}
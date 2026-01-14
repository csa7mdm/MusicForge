using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MusicForge.Application.Interfaces;
using MusicForge.Cli.Commands;
using MusicForge.Infrastructure.Persistence;
using Spectre.Console.Cli;

// Build DI container
// Build DI container
var services = new ServiceCollection();

var outputDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
var dbPath = Path.Combine(outputDir!, "MusicForge.db");
var connectionString = $"Data Source={dbPath}";

services.AddDbContext<MusicForgeDbContext>(options =>
    options.UseSqlite(connectionString));

services.AddScoped<IProjectRepository, SqliteProjectRepository>();

var registrar = new TypeRegistrar(services);

// Build CLI app
var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.SetApplicationName("musicforge");
    config.SetApplicationVersion("0.1.0");

    config.AddCommand<NewCommand>("new")
    .WithDescription("Create a new music project")
    .WithExample("new", "-i")
    .WithExample("new", "MySong", "-g", "electronic", "-t", "128");

    config.AddCommand<ListCommand>("list")
    .WithDescription("List all projects")
    .WithAlias("ls")
    .WithExample("list")
    .WithExample("list", "--json");

    config.AddCommand<GenerateCommand>("generate")
    .WithDescription("Generate music for a project")
    .WithAlias("gen")
    .WithExample("generate", "<project-id>");

    config.AddCommand<StatusCommand>("status")
    .WithDescription("Show project details")
    .WithAlias("show")
    .WithExample("status", "<project-id>");
});

return await app.RunAsync(args);

/// <summary>
/// Spectre.Console.Cli type registrar for DI.
/// </summary>
public sealed class TypeRegistrar(IServiceCollection services) : ITypeRegistrar
{
    private IServiceProvider? _provider;

    public ITypeResolver Build()
    {
        _provider = services.BuildServiceProvider();

        // Ensure DB created
        using (var scope = _provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MusicForgeDbContext>();
            db.Database.EnsureCreated();
        }

        return new TypeResolver(_provider);
    }

    public void Register(Type service, Type implementation)
    {
        services.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type service, object implementation)
    {
        services.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        services.AddSingleton(service, _ => factory());
    }
}

/// <summary>
/// Type resolver for DI.
/// </summary>
public sealed class TypeResolver(IServiceProvider provider) : ITypeResolver
{
    public object? Resolve(Type? type)
    {
        return type is null ? null : provider.GetService(type) ?? ActivatorUtilities.CreateInstance(provider, type);
    }
}

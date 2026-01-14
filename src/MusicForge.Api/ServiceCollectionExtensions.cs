using Microsoft.EntityFrameworkCore;
// using MusicForge.Infrastructure.Worker; // Removed
using Microsoft.Extensions.DependencyInjection; // Explicitly ensure extensions are available
using MusicForge.Application.Commands;
using MusicForge.Application.Interfaces;
using MusicForge.Infrastructure.Agent;
using MusicForge.Infrastructure.Llm;
using MusicForge.Infrastructure.Persistence;

namespace MusicForge.Api;

/// <summary>
/// Dependency injection configuration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add MusicForge application services.
    /// </summary>
    public static IServiceCollection AddMusicForgeServices(this IServiceCollection services, IConfiguration configuration)
    {
        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateProjectCommand>());

        // Repositories
        // Repositories & Database
        var outputDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        var dbPath = Path.Combine(outputDir!, "MusicForge.db");
        var connectionString = $"Data Source={dbPath}";

        services.AddDbContext<MusicForgeDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IProjectRepository, SqliteProjectRepository>();

        // HTTP clients for LLM providers
        services.AddHttpClient("Groq", client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY");
            if (!string.IsNullOrEmpty(apiKey))
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        });

        services.AddHttpClient("OpenRouter", client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
            if (!string.IsNullOrEmpty(apiKey))
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        });

        // LLM Client Factory
        services.AddSingleton<ILlmClientFactory>(sp =>
        {
            var httpFactory = sp.GetRequiredService<IHttpClientFactory>();
            var defaultProvider = Environment.GetEnvironmentVariable("LLM_PROVIDER") switch
            {
                "groq" => LlmProvider.Groq,
                "openrouter" => LlmProvider.OpenRouter,
                "gemini" => LlmProvider.Gemini,
                "deepseek" => LlmProvider.DeepSeek,
                "ollama" => LlmProvider.Ollama,
                _ => LlmProvider.Groq
            };
            return new LlmClientFactory(sp, httpFactory, defaultProvider);
        });

        // Register Agent Orchestrator
        services.AddAgentOrchestrator();

        // Register Music Worker Client
        var workerAddress = configuration["MusicWorker:Address"] ?? "http://localhost:50051";
        services.AddMusicWorkerClient(workerAddress);

        return services;
    }
    /// <summary>
    /// Add gRPC music worker client.
    /// </summary>
    public static IServiceCollection AddMusicWorkerClient(
        this IServiceCollection services,
        string workerAddress = "http://localhost:50051")
    {
        services.AddGrpcClient<MusicForge.Infrastructure.Grpc.MusicWorker.MusicWorkerClient>(o =>
        {
            o.Address = new Uri(workerAddress);
        });

        services.AddScoped<IMusicWorkerClient, MusicForge.Infrastructure.Grpc.GrpcMusicWorkerClient>();

        return services;
    }
    public static IServiceCollection AddAgentOrchestrator(this IServiceCollection services)
    {
        services.AddScoped<MusicForge.Application.Interfaces.IAgentOrchestrator, MusicForge.Infrastructure.Agent.MusicAgentOrchestrator>();
        // services.AddSingleton<IAgentStateMachine, MusicAgentStateMachine>(); // Placeholder
        return services;
    }
}

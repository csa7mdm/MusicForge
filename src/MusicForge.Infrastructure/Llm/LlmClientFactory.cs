using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using MusicForge.Application.Interfaces;

namespace MusicForge.Infrastructure.Llm;

/// <summary>
/// Factory for creating LLM clients based on provider.
/// </summary>
public sealed class LlmClientFactory : ILlmClientFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly LlmProvider _defaultProvider;

    public LlmClientFactory(
    IServiceProvider serviceProvider,
    IHttpClientFactory httpClientFactory,
    LlmProvider defaultProvider = LlmProvider.Groq)
    {
        _serviceProvider = serviceProvider;
        _httpClientFactory = httpClientFactory;
        _defaultProvider = defaultProvider;
    }

    public ILlmClient CreateClient(LlmProvider provider)
    {
        var httpClient = _httpClientFactory.CreateClient(provider.ToString());

        return provider switch
        {
            LlmProvider.Groq => new GroqClient(httpClient),
            LlmProvider.OpenRouter => new OpenRouterClient(httpClient),
            LlmProvider.Gemini => new GeminiClient(httpClient),
            LlmProvider.DeepSeek => new DeepSeekClient(httpClient),
            LlmProvider.Ollama => new OllamaClient(httpClient),
            _ => throw new NotSupportedException($"Provider {provider} is not supported.")
        };
    }

    public ILlmClient GetDefaultClient() => CreateClient(_defaultProvider);

    public IReadOnlyList<LlmProvider> GetAvailableProviders() =>
    [LlmProvider.Groq, LlmProvider.OpenRouter, LlmProvider.Gemini, LlmProvider.DeepSeek, LlmProvider.Ollama];
}

/// <summary>
/// LLM client for Google Gemini API.
/// </summary>
public sealed class GeminiClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    public LlmProvider Provider => LlmProvider.Gemini;

    public GeminiClient(HttpClient httpClient) => _httpClient = httpClient;

    public Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken ct = default)
    {
        // Placeholder - implement Gemini API
        throw new NotImplementedException("Gemini client not yet implemented.");
    }

    public IAsyncEnumerable<string> StreamCompleteAsync(LlmRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException("Gemini streaming not yet implemented.");
    }
}

/// <summary>
/// LLM client for DeepSeek API.
/// </summary>
public sealed class DeepSeekClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    public LlmProvider Provider => LlmProvider.DeepSeek;

    public DeepSeekClient(HttpClient httpClient) => _httpClient = httpClient;

    public Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken ct = default)
    {
        // Placeholder - implement DeepSeek API (OpenAI-compatible)
        throw new NotImplementedException("DeepSeek client not yet implemented.");
    }

    public IAsyncEnumerable<string> StreamCompleteAsync(LlmRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException("DeepSeek streaming not yet implemented.");
    }
}

/// <summary>
/// LLM client for local Ollama.
/// </summary>
public sealed class OllamaClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public LlmProvider Provider => LlmProvider.Ollama;

    public OllamaClient(HttpClient httpClient, string model = "llama3.2")
    {
        _httpClient = httpClient;
        _model = model;
    }

    public Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken ct = default)
    {
        // Placeholder - implement Ollama API
        throw new NotImplementedException("Ollama client not yet implemented.");
    }

    public IAsyncEnumerable<string> StreamCompleteAsync(LlmRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException("Ollama streaming not yet implemented.");
    }
}

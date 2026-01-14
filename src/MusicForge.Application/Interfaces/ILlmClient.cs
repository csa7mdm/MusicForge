namespace MusicForge.Application.Interfaces;

/// <summary>
/// LLM provider options.
/// </summary>
public enum LlmProvider
{
    Ollama,
    Groq,
    OpenRouter,
    Gemini,
    DeepSeek
}

/// <summary>
/// LLM completion request.
/// </summary>
public sealed record LlmRequest(
string Prompt,
string? SystemPrompt = null,
float Temperature = 0.7f,
int MaxTokens = 2048
);

/// <summary>
/// LLM completion response.
/// </summary>
public sealed record LlmResponse(
string Content,
int PromptTokens,
int CompletionTokens,
TimeSpan Duration
);

/// <summary>
/// Client for interacting with LLM providers.
/// </summary>
public interface ILlmClient
{
    /// <summary>
    /// The provider this client uses.
    /// </summary>
    LlmProvider Provider { get; }

    /// <summary>
    /// Complete a prompt.
    /// </summary>
    Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken ct = default);

    /// <summary>
    /// Stream completion tokens.
    /// </summary>
    IAsyncEnumerable<string> StreamCompleteAsync(LlmRequest request, CancellationToken ct = default);
}

/// <summary>
/// Factory for creating LLM clients.
/// </summary>
public interface ILlmClientFactory
{
    /// <summary>
    /// Create a client for the specified provider.
    /// </summary>
    ILlmClient CreateClient(LlmProvider provider);

    /// <summary>
    /// Get the default/preferred client.
    /// </summary>
    ILlmClient GetDefaultClient();

    /// <summary>
    /// Get available providers.
    /// </summary>
    IReadOnlyList<LlmProvider> GetAvailableProviders();
}

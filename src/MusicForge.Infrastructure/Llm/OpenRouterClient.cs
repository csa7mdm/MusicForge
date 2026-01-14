using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MusicForge.Application.Interfaces;

namespace MusicForge.Infrastructure.Llm;

/// <summary>
/// LLM client for OpenRouter API.
/// </summary>
public sealed class OpenRouterClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public LlmProvider Provider => LlmProvider.OpenRouter;

    public OpenRouterClient(HttpClient httpClient, string model = "anthropic/claude-3.5-sonnet")
    {
        _httpClient = httpClient;
        _model = model;
    }

    public async Task<LlmResponse> CompleteAsync(LlmRequest request, CancellationToken ct = default)
    {
        var startTime = DateTime.UtcNow;

        var messages = new List<object>();
        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            messages.Add(new { role = "system", content = request.SystemPrompt });
        }
        messages.Add(new { role = "user", content = request.Prompt });

        var payload = new
        {
            model = _model,
            messages,
            temperature = request.Temperature,
            max_tokens = request.MaxTokens
        };

        var response = await _httpClient.PostAsJsonAsync(
        "https://openrouter.ai/api/v1/chat/completions",
        payload,
        ct);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<OpenRouterResponse>(ct);

        return new LlmResponse(
        Content: result?.Choices?.FirstOrDefault()?.Message?.Content ?? "",
        PromptTokens: result?.Usage?.PromptTokens ?? 0,
        CompletionTokens: result?.Usage?.CompletionTokens ?? 0,
        Duration: DateTime.UtcNow - startTime
        );
    }

    public async IAsyncEnumerable<string> StreamCompleteAsync(
    LlmRequest request,
    [EnumeratorCancellation] CancellationToken ct = default)
    {
        var messages = new List<object>();
        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            messages.Add(new { role = "system", content = request.SystemPrompt });
        }
        messages.Add(new { role = "user", content = request.Prompt });

        var payload = new { model = _model, messages, temperature = request.Temperature, max_tokens = request.MaxTokens, stream = true };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions") { Content = content };

        var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: ")) continue;

            var data = line["data: ".Length..];
            if (data == "[DONE]") break;

            var chunk = JsonSerializer.Deserialize<OpenRouterStreamChunk>(data);
            var delta = chunk?.Choices?.FirstOrDefault()?.Delta?.Content;
            if (!string.IsNullOrEmpty(delta)) yield return delta;
        }
    }

    private sealed record OpenRouterResponse(
    [property: JsonPropertyName("choices")] List<OpenRouterChoice>? Choices,
    [property: JsonPropertyName("usage")] OpenRouterUsage? Usage);

    private sealed record OpenRouterChoice([property: JsonPropertyName("message")] OpenRouterMessage? Message);
    private sealed record OpenRouterMessage([property: JsonPropertyName("content")] string? Content);
    private sealed record OpenRouterUsage([property: JsonPropertyName("prompt_tokens")] int PromptTokens, [property: JsonPropertyName("completion_tokens")] int CompletionTokens);
    private sealed record OpenRouterStreamChunk([property: JsonPropertyName("choices")] List<OpenRouterStreamChoice>? Choices);
    private sealed record OpenRouterStreamChoice([property: JsonPropertyName("delta")] OpenRouterDelta? Delta);
    private sealed record OpenRouterDelta([property: JsonPropertyName("content")] string? Content);
}

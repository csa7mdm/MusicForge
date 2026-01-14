using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MusicForge.Application.Interfaces;

namespace MusicForge.Infrastructure.Llm;

/// <summary>
/// LLM client for Groq API.
/// </summary>
public sealed class GroqClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public LlmProvider Provider => LlmProvider.Groq;

    public GroqClient(HttpClient httpClient, string model = "llama-3.3-70b-versatile")
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
            max_tokens = request.MaxTokens,
            stream = false
        };

        var response = await _httpClient.PostAsJsonAsync(
        "https://api.groq.com/openai/v1/chat/completions",
        payload,
        ct);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GroqResponse>(ct);

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

        var payload = new
        {
            model = _model,
            messages,
            temperature = request.Temperature,
            max_tokens = request.MaxTokens,
            stream = true
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions")
        {
            Content = content
        };

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

            var chunk = JsonSerializer.Deserialize<GroqStreamChunk>(data);
            var delta = chunk?.Choices?.FirstOrDefault()?.Delta?.Content;
            if (!string.IsNullOrEmpty(delta))
            {
                yield return delta;
            }
        }
    }

    private sealed record GroqResponse(
    [property: JsonPropertyName("choices")] List<GroqChoice>? Choices,
    [property: JsonPropertyName("usage")] GroqUsage? Usage
    );

    private sealed record GroqChoice(
    [property: JsonPropertyName("message")] GroqMessage? Message
    );

    private sealed record GroqMessage(
    [property: JsonPropertyName("content")] string? Content
    );

    private sealed record GroqUsage(
    [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
    [property: JsonPropertyName("completion_tokens")] int CompletionTokens
    );

    private sealed record GroqStreamChunk(
    [property: JsonPropertyName("choices")] List<GroqStreamChoice>? Choices
    );

    private sealed record GroqStreamChoice(
    [property: JsonPropertyName("delta")] GroqDelta? Delta
    );

    private sealed record GroqDelta(
    [property: JsonPropertyName("content")] string? Content
    );
}

using MusicForge.Domain.Enums;
using MusicForge.Domain.ValueObjects;

namespace MusicForge.Domain.Entities;

/// <summary>
/// Represents a turn in the conversation history.
/// </summary>
public sealed record ConversationTurn(string Role, string Content, DateTime Timestamp);

/// <summary>
/// Tracks the LLM agent's context, conversation history, and focus checkpoints to prevent drift.
/// </summary>
public sealed class AgentContext
{
    /// <summary>
    /// Unique session identifier.
    /// </summary>
    public Guid SessionId { get; }

    private readonly List<ConversationTurn> _history = [];

    /// <summary>
    /// Conversation history with the LLM.
    /// </summary>
    public IReadOnlyList<ConversationTurn> History => _history.AsReadOnly();

    /// <summary>
    /// Current parsed intent from the latest user input.
    /// </summary>
    public string? CurrentIntent { get; private set; }

    /// <summary>
    /// Overall task progress (0.0 to 1.0).
    /// </summary>
    public float TaskProgress { get; private set; }

    /// <summary>
    /// The established musical key for this session.
    /// </summary>
    public MusicalKey? EstablishedKey { get; private set; }

    /// <summary>
    /// The established tempo for this session.
    /// </summary>
    public BpmTempo? EstablishedTempo { get; private set; }

    /// <summary>
    /// The established genre for this session.
    /// </summary>
    public Genre? EstablishedGenre { get; private set; }

    private readonly List<string> _focusCheckpoints = [];

    /// <summary>
    /// Checkpoints to verify the agent stays on track.
    /// </summary>
    public IReadOnlyList<string> FocusCheckpoints => _focusCheckpoints.AsReadOnly();

    public AgentContext()
    {
        SessionId = Guid.NewGuid();
    }

    /// <summary>
    /// Adds a conversation turn to history.
    /// </summary>
    public void AddTurn(string role, string content)
    {
        _history.Add(new ConversationTurn(role, content, DateTime.UtcNow));
    }

    /// <summary>
    /// Sets the current intent.
    /// </summary>
    public void SetIntent(string intent)
    {
        CurrentIntent = intent;
        AddFocusCheckpoint($"Intent: {intent}");
    }

    /// <summary>
    /// Updates task progress.
    /// </summary>
    public void UpdateProgress(float progress)
    {
        TaskProgress = Math.Clamp(progress, 0f, 1f);
    }

    /// <summary>
    /// Establishes key musical parameters.
    /// </summary>
    public void EstablishMusicalContext(MusicalKey key, BpmTempo tempo, Genre genre)
    {
        EstablishedKey = key;
        EstablishedTempo = tempo;
        EstablishedGenre = genre;
        AddFocusCheckpoint($"Established: {key}, {tempo}, {genre}");
    }

    /// <summary>
    /// Adds a focus checkpoint to prevent model drift.
    /// </summary>
    public void AddFocusCheckpoint(string checkpoint)
    {
        _focusCheckpoints.Add(checkpoint);

        // Keep only last 10 checkpoints
        if (_focusCheckpoints.Count > 10)
        {
            _focusCheckpoints.RemoveAt(0);
        }
    }

    /// <summary>
    /// Checks if the current action indicates potential drift from the original intent.
    /// </summary>
    public bool CheckFocusDrift(string currentAction)
    {
        // Simple heuristic: if current action is very different from checkpoints, flag drift
        if (_focusCheckpoints.Count == 0 || string.IsNullOrWhiteSpace(CurrentIntent))
            return false;

        // Could be enhanced with semantic similarity checking via LLM
        return false;
    }

    /// <summary>
    /// Generates context for LLM prompt injection.
    /// </summary>
    public string ToPromptContext()
    {
        var parts = new List<string>();

        if (EstablishedGenre != null)
            parts.Add($"Genre: {EstablishedGenre}");
        if (EstablishedKey != null)
            parts.Add($"Key: {EstablishedKey}");
        if (EstablishedTempo != null)
            parts.Add($"Tempo: {EstablishedTempo}");
        if (!string.IsNullOrWhiteSpace(CurrentIntent))
            parts.Add($"Current intent: {CurrentIntent}");
        if (_focusCheckpoints.Count > 0)
            parts.Add($"Focus: {string.Join(", ", _focusCheckpoints.TakeLast(3))}");

        return parts.Count > 0 ? string.Join("\n", parts) : "No context established.";
    }
}

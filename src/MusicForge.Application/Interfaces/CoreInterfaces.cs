using MusicForge.Application.Commands;
using MusicForge.Domain.Entities;

namespace MusicForge.Application.Interfaces;

/// <summary>
/// Agent state definitions.
/// </summary>
public enum AgentState
{
    Idle,
    Understanding,
    Planning,
    Composing,
    GeneratingMidi,
    SynthesizingAudio,
    SynthesizingVocals,
    Mixing,
    Mastering,
    Exporting,
    AwaitingFeedback,
    Iterating,
    Error,
    Complete
}

/// <summary>
/// Parsed user intent from natural language.
/// </summary>
public sealed record ParsedIntent(
string Action,
float Confidence,
string? TargetSection,
IReadOnlyDictionary<string, object> Parameters
);

/// <summary>
/// Execution plan for the agent.
/// </summary>
public sealed record ExecutionPlan(
IReadOnlyList<ExecutionStep> Steps,
int EstimatedDurationSeconds
);

/// <summary>
/// Single step in an execution plan.
/// </summary>
public sealed record ExecutionStep(
string Name,
AgentState TargetState,
IReadOnlyDictionary<string, object> Parameters
);

/// <summary>
/// Progress update during generation.
/// </summary>
public sealed record ProgressUpdate(
AgentState State,
string Component,
float Progress,
string Message
);

/// <summary>
/// Result of a generation request.
/// </summary>
public sealed record GenerationResult(
    bool Success,
    string? ErrorMessage,
    string? MasterFilePath,
    IReadOnlyList<string> StemPaths
);



/// <summary>
/// State machine for agent transitions.
/// </summary>
public interface IAgentStateMachine
{
    AgentState CurrentState { get; }
    IReadOnlyList<(AgentState State, DateTime Timestamp)> StateHistory { get; }
    Task<AgentState> TransitionAsync(AgentContext context, CancellationToken ct = default);
    bool CanTransitionTo(AgentState targetState);
    void Reset();
}

/// <summary>
/// Parses user input into structured intent.
/// </summary>
public interface IIntentParser
{
    Task<ParsedIntent> ParseAsync(string userInput, AgentContext context, CancellationToken ct = default);
}

/// <summary>
/// Creates execution plans from parsed intents.
/// </summary>
public interface ITaskPlanner
{
    ExecutionPlan CreatePlan(ParsedIntent intent, AgentContext context);
}

/// <summary>
/// Main orchestrator for music generation.
/// </summary>
public interface IAgentOrchestrator
{
    Task<GenerationResult> GenerateAsync(Project project, string prompt, CancellationToken ct = default);
    Task<GenerationResult> IterateAsync(Project project, string feedback, string? targetSection, CancellationToken ct = default);
    IAsyncEnumerable<ProgressUpdate> StreamProgressAsync(ProjectId projectId, CancellationToken ct = default);
}

/// <summary>
/// Repository for project persistence.
/// </summary>
public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(ProjectId id, CancellationToken ct = default);
    Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default);
    Task SaveAsync(Project project, CancellationToken ct = default);
    Task DeleteAsync(ProjectId id, CancellationToken ct = default);
}

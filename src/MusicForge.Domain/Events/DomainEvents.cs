using MusicForge.Domain.Abstractions;
using MusicForge.Domain.Entities;

namespace MusicForge.Domain.Events;

/// <summary>
/// Raised when a new project is created.
/// </summary>
public sealed record ProjectCreatedEvent(ProjectId ProjectId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// Raised when generation starts for a project.
/// </summary>
public sealed record GenerationStartedEvent(ProjectId ProjectId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// Raised when a stem is completed.
/// </summary>
public sealed record StemCompletedEvent(ProjectId ProjectId, StemId StemId, string StemName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// Raised when project generation completes successfully.
/// </summary>
public sealed record ProjectCompletedEvent(ProjectId ProjectId, string MasterFilePath) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// Raised when project generation fails.
/// </summary>
public sealed record ProjectFailedEvent(ProjectId ProjectId, string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// Raised when project status changes.
/// </summary>
public sealed record ProjectStatusChangedEvent(ProjectId ProjectId, Enums.ProjectStatus NewStatus) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

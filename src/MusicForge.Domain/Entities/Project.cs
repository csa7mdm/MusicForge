using MusicForge.Domain.Abstractions;
using MusicForge.Domain.Enums;
using MusicForge.Domain.Events;
using MusicForge.Domain.ValueObjects;

namespace MusicForge.Domain.Entities;

/// <summary>
/// Strongly-typed ID for Project aggregate.
/// </summary>
public sealed record ProjectId(Guid Value) : EntityId(Value)
{
    public static ProjectId New() => new(Guid.NewGuid());
}

/// <summary>
/// The Project aggregate root - the central entity for music generation.
/// </summary>
public sealed class Project : AggregateRoot<ProjectId>
{
    private readonly List<Stem> _stems = [];

    /// <summary>
    /// Project name.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The song specification.
    /// </summary>
    public SongSpecification Specification { get; private set; }

    /// <summary>
    /// The generated arrangement (null until composition complete).
    /// </summary>
    public Arrangement? Arrangement { get; private set; }

    /// <summary>
    /// Generated audio stems.
    /// </summary>
    public IReadOnlyList<Stem> Stems => _stems.AsReadOnly();

    /// <summary>
    /// Path to the final mastered audio file.
    /// </summary>
    public string? MasterFilePath { get; private set; }

    /// <summary>
    /// Current status in the generation pipeline.
    /// </summary>
    public ProjectStatus Status { get; private set; }

    /// <summary>
    /// LLM agent context for this project.
    /// </summary>
    public AgentContext Context { get; private set; }

    /// <summary>
    /// When the project was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// When the project was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Error message if status is Failed.
    /// </summary>
    public string? FailureReason { get; private set; }

    private Project() : base()
    {
        Name = string.Empty;
        Specification = null!;
        Context = new AgentContext();
    }

    private Project(ProjectId id, string name, SongSpecification specification) : base(id)
    {
        Name = name;
        Specification = specification;
        Status = ProjectStatus.Draft;
        Context = new AgentContext();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new project.
    /// </summary>
    public static Project Create(string name, SongSpecification specification)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(specification);

        var project = new Project(ProjectId.New(), name, specification);
        project.RaiseDomainEvent(new ProjectCreatedEvent(project.Id));
        return project;
    }

    /// <summary>
    /// Starts the generation process.
    /// </summary>
    public void StartGeneration()
    {
        if (Status != ProjectStatus.Draft)
            throw new InvalidOperationException($"Cannot start generation from status {Status}.");

        TransitionTo(ProjectStatus.Composing);
        Context.AddFocusCheckpoint($"Generation started: {Specification.Description}");
        Context.EstablishMusicalContext(Specification.Key, Specification.Tempo, Specification.Genre);
        RaiseDomainEvent(new GenerationStartedEvent(Id));
    }

    /// <summary>
    /// Updates the arrangement.
    /// </summary>
    public void UpdateArrangement(Arrangement arrangement)
    {
        Arrangement = arrangement;
        Touch();
    }

    /// <summary>
    /// Adds a completed stem.
    /// </summary>
    public void AddStem(Stem stem)
    {
        _stems.Add(stem);
        Touch();
        RaiseDomainEvent(new StemCompletedEvent(Id, stem.Id, stem.Name));
    }

    /// <summary>
    /// Sets the final master file path.
    /// </summary>
    public void SetMasterFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        MasterFilePath = path;
        Touch();
    }

    /// <summary>
    /// Transitions to a new status.
    /// </summary>
    public void TransitionTo(ProjectStatus newStatus)
    {
        if (Status == ProjectStatus.Complete || Status == ProjectStatus.Failed)
            throw new InvalidOperationException($"Cannot transition from terminal status {Status}.");

        Status = newStatus;
        Touch();
        RaiseDomainEvent(new ProjectStatusChangedEvent(Id, newStatus));
    }

    /// <summary>
    /// Marks the project as complete.
    /// </summary>
    public void Complete()
    {
        if (string.IsNullOrWhiteSpace(MasterFilePath))
            throw new InvalidOperationException("Cannot complete without a master file.");

        Status = ProjectStatus.Complete;
        Touch();
        RaiseDomainEvent(new ProjectCompletedEvent(Id, MasterFilePath));
    }

    /// <summary>
    /// Marks the project as failed.
    /// </summary>
    public void Fail(string reason)
    {
        FailureReason = reason;
        Status = ProjectStatus.Failed;
        Touch();
        RaiseDomainEvent(new ProjectFailedEvent(Id, reason));
    }

    /// <summary>
    /// Updates the progress in the agent context.
    /// </summary>
    public void UpdateProgress(float progress)
    {
        Context.UpdateProgress(progress);
        Touch();
    }

    private void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

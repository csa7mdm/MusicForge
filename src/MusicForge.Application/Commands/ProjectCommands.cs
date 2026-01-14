using MediatR;
using MusicForge.Application.Interfaces;
using MusicForge.Domain.Entities;
using MusicForge.Domain.Enums;

namespace MusicForge.Application.Commands;

/// <summary>
/// Command to create a new music project.
/// </summary>
public sealed record CreateProjectCommand(
string Name,
string Description,
Genre Genre,
Mood Mood,
int TempoBpm,
string Key,
int DurationSeconds,
bool HasVocals,
string? Lyrics
) : IRequest<ProjectId>;

/// <summary>
/// Command to start or regenerate music for a project.
/// </summary>
public sealed record GenerateSongCommand(
ProjectId ProjectId,
string Prompt
) : IRequest<GenerationResult>;

/// <summary>
/// Command to iterate on existing generation with feedback.
/// </summary>
public sealed record IterateOnFeedbackCommand(
ProjectId ProjectId,
string Feedback,
string? TargetSection
) : IRequest<GenerationResult>;

/// <summary>
/// Command to export a project to various formats.
/// </summary>
public sealed record ExportProjectCommand(
ProjectId ProjectId,
ExportFormat Format,
bool IncludeStems
) : IRequest<ExportResult>;



/// <summary>
/// Result of an export operation.
/// </summary>
public sealed record ExportResult(
bool Success,
string? ErrorMessage,
string ExportPath,
long FileSizeBytes
);

/// <summary>
/// Export format options.
/// </summary>
public enum ExportFormat
{
    Wav,
    Mp3,
    Flac,
    Ogg
}

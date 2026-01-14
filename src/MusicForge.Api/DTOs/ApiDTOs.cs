using MusicForge.Domain.Enums;

namespace MusicForge.Api.DTOs;

/// <summary>
/// Request to create a new music project.
/// </summary>
public sealed record CreateProjectRequest(
string Name,
string Description,
string Genre,
string Mood,
int TempoBpm,
string Key,
int DurationSeconds,
bool HasVocals = false,
string? Lyrics = null
);

/// <summary>
/// Response after project creation.
/// </summary>
public sealed record CreateProjectResponse(Guid ProjectId, string Message);

/// <summary>
/// Request to generate music.
/// </summary>
public sealed record GenerateRequest(string Prompt);

/// <summary>
/// Request to iterate on existing generation.
/// </summary>
public sealed record IterateRequest(string Feedback, string? TargetSection = null);

/// <summary>
/// Generation result response.
/// </summary>
public sealed record GenerationResponse(
bool Success,
string? ErrorMessage,
string? MasterFilePath,
IReadOnlyList<string> StemPaths
);

/// <summary>
/// Project summary for list view.
/// </summary>
public sealed record ProjectSummary(
Guid Id,
string Name,
string Status,
string Genre,
int TempoBpm,
int DurationSeconds,
DateTime CreatedAt,
DateTime UpdatedAt
);

/// <summary>
/// Detailed project view.
/// </summary>
public sealed record ProjectDetails(
Guid Id,
string Name,
string Description,
string Status,
string Genre,
string Mood,
int TempoBpm,
string Key,
int DurationSeconds,
bool HasVocals,
string? Lyrics,
string? MasterFilePath,
IReadOnlyList<StemInfo> Stems,
ArrangementInfo? Arrangement,
DateTime CreatedAt,
DateTime UpdatedAt
);

/// <summary>
/// Stem information.
/// </summary>
public sealed record StemInfo(string Name, string Path, TimeSpan Duration);

/// <summary>
/// Arrangement information.
/// </summary>
public sealed record ArrangementInfo(
IReadOnlyList<string> ChordProgression,
int TotalBars,
IReadOnlyList<SectionInfo> Sections
);

/// <summary>
/// Section information.
/// </summary>
public sealed record SectionInfo(
string Name,
int StartBar,
int DurationBars,
float EnergyLevel,
IReadOnlyList<string> Elements
);

/// <summary>
/// Real-time progress update.
/// </summary>
public sealed record ProgressUpdate(
string State,
string Component,
float Progress,
string Message
);

/// <summary>
/// Health check response.
/// </summary>
public sealed record HealthResponse(
string Status,
string Version,
bool WorkerConnected,
bool GpuAvailable
);

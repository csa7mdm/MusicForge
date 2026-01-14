namespace MusicForge.Application.Interfaces;

/// <summary>
/// Theory generation request.
/// </summary>
public sealed record TheoryRequest(
string Genre,
string Mood,
int TempoBpm,
string Key,
string Mode,
int DurationSeconds,
IReadOnlyList<string> StyleTags
);

/// <summary>
/// Theory generation result.
/// </summary>
public sealed record TheoryResult(
IReadOnlyList<string> ChordProgression,
IReadOnlyList<SectionData> Sections,
byte[] MidiData
);

/// <summary>
/// Section data from theory generation.
/// </summary>
public sealed record SectionData(
string Name,
int StartBar,
int DurationBars,
float EnergyLevel,
IReadOnlyList<string> Elements
);

/// <summary>
/// Audio synthesis request.
/// </summary>
public sealed record AudioRequest(
string Prompt,
int DurationSeconds,
string Genre,
float EnergyLevel,
byte[]? ConditioningAudio,
string SectionName
);

/// <summary>
/// Audio chunk for streaming response.
/// </summary>
public sealed record AudioChunk(
byte[] AudioData,
int SampleRate,
bool IsFinal,
float Progress
);

/// <summary>
/// Vocal synthesis request.
/// </summary>
public sealed record VocalRequest(
string Lyrics,
string VoiceType,
string Style,
int TargetDurationMs
);

/// <summary>
/// Stem separation result.
/// </summary>
public sealed record StemSeparationResult(
byte[] Drums,
byte[] Bass,
byte[] Vocals,
byte[] Other,
int SampleRate
);

/// <summary>
/// Worker health status.
/// </summary>
public sealed record HealthStatus(
string Status,
bool GpuAvailable,
long GpuMemoryBytes,
IReadOnlyList<string> ModelsLoaded
);

/// <summary>
/// Client for communicating with Python AI workers via gRPC.
/// </summary>
public interface IMusicWorkerClient
{
    /// <summary>
    /// Generate music theory elements.
    /// </summary>
    Task<TheoryResult> GenerateTheoryAsync(TheoryRequest request, CancellationToken ct = default);

    /// <summary>
    /// Synthesize audio with streaming chunks.
    /// </summary>
    IAsyncEnumerable<AudioChunk> SynthesizeAudioAsync(AudioRequest request, CancellationToken ct = default);

    /// <summary>
    /// Synthesize vocals with streaming chunks.
    /// </summary>
    IAsyncEnumerable<AudioChunk> SynthesizeVocalsAsync(VocalRequest request, CancellationToken ct = default);

    /// <summary>
    /// Separate audio into stems.
    /// </summary>
    Task<StemSeparationResult> SeparateStemsAsync(byte[] audioData, CancellationToken ct = default);

    /// <summary>
    /// Check worker health status.
    /// </summary>
    Task<HealthStatus> HealthCheckAsync(CancellationToken ct = default);
}

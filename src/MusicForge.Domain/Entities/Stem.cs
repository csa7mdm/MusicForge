using MusicForge.Domain.Abstractions;
using MusicForge.Domain.Enums;

namespace MusicForge.Domain.Entities;

/// <summary>
/// Strongly-typed ID for Stem entity.
/// </summary>
public sealed record StemId(Guid Value) : EntityId(Value)
{
    public static StemId New() => new(Guid.NewGuid());
}

/// <summary>
/// Represents an individual audio stem (drums, bass, vocals, etc.).
/// </summary>
public sealed class Stem : Entity<StemId>
{
    /// <summary>
    /// Name of the stem (e.g., "drums", "bass", "vocals", "lead", "pad", "fx").
    /// </summary>
    /// <summary>
    /// Name of the stem (e.g., "drums", "bass", "vocals", "lead", "pad", "fx").
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The type of stem.
    /// </summary>
    public ComponentType Type { get; private set; }

    /// <summary>
    /// File path to the audio file.
    /// </summary>
    public string Path { get; private set; }

    /// <summary>
    /// Duration of the audio.
    /// </summary>
    public TimeSpan Duration { get; private set; }

    /// <summary>
    /// Audio sample rate in Hz.
    /// </summary>
    public int SampleRate { get; private set; }

    /// <summary>
    /// Number of audio channels (1 = mono, 2 = stereo).
    /// </summary>
    public int Channels { get; private set; }

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSizeBytes { get; private set; }

    private Stem() : base()
    {
        Name = string.Empty;
        Path = string.Empty;
    }

    private Stem(
    StemId id,
    string name,
    ComponentType type,
    string path,
    TimeSpan duration,
    int sampleRate,
    int channels,
    long fileSizeBytes) : base(id)
    {
        Name = name;
        Type = type;
        Path = path;
        Duration = duration;
        SampleRate = sampleRate;
        Channels = channels;
        FileSizeBytes = fileSizeBytes;
    }

    /// <summary>
    /// Creates a new stem.
    /// </summary>
    public static Stem Create(
    string name,
    ComponentType type,
    string path,
    TimeSpan? duration = null,
    int sampleRate = 44100,
    int channels = 2,
    long fileSizeBytes = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        return new Stem(StemId.New(), name, type, path, duration ?? TimeSpan.Zero, sampleRate, channels, fileSizeBytes);
    }

    /// <summary>
    /// Updates file metadata after processing.
    /// </summary>
    public void UpdateMetadata(TimeSpan duration, long fileSizeBytes)
    {
        Duration = duration;
        FileSizeBytes = fileSizeBytes;
    }
}

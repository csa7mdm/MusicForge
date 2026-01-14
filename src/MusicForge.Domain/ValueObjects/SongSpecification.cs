using MusicForge.Domain.Enums;

namespace MusicForge.Domain.ValueObjects;

/// <summary>
/// Complete specification for a song to be generated.
/// </summary>
public sealed class SongSpecification
{
    /// <summary>
    /// Text description of the desired song.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Musical genre.
    /// </summary>
    public Genre Genre { get; }

    /// <summary>
    /// Emotional mood.
    /// </summary>
    public Mood Mood { get; }

    /// <summary>
    /// Tempo in BPM.
    /// </summary>
    public BpmTempo Tempo { get; }

    /// <summary>
    /// Musical key.
    /// </summary>
    public MusicalKey Key { get; }

    /// <summary>
    /// Time signature.
    /// </summary>
    public TimeSignature TimeSignature { get; }

    /// <summary>
    /// Target duration in seconds.
    /// </summary>
    public int DurationSeconds { get; }

    /// <summary>
    /// Whether the song should include synthesized vocals.
    /// </summary>
    public bool HasVocals { get; }

    /// <summary>
    /// Lyrics if vocals are enabled.
    /// </summary>
    public string? Lyrics { get; }

    /// <summary>
    /// Additional style tags for fine-tuning generation.
    /// </summary>
    public IReadOnlyList<string> StyleTags { get; }

    /// <summary>
    /// Creates a new song specification.
    /// </summary>
    public SongSpecification(
        string description,
        Genre genre,
        Mood mood,
        BpmTempo tempo,
        MusicalKey? key = null,
        TimeSignature? timeSignature = null,
        int durationSeconds = 180,
        bool hasVocals = false,
        string? lyrics = null,
        IReadOnlyList<string>? styleTags = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        if (durationSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Duration must be positive.");

        Description = description;
        Genre = genre;
        Mood = mood;
        Tempo = tempo;
        Key = key ?? new MusicalKey(Note.C, Mode.Major);
        TimeSignature = timeSignature ?? ValueObjects.TimeSignature.Common;
        DurationSeconds = durationSeconds;
        HasVocals = hasVocals;
        Lyrics = hasVocals ? lyrics : null;
        StyleTags = styleTags ?? [];
    }

    // Constructor for EF Core
    private SongSpecification()
    {
        Description = string.Empty;
        Genre = Genre.Electronic;
        Mood = Mood.Chill;
        Tempo = new BpmTempo(120);
        Key = new MusicalKey(Note.C, Mode.Major);
        TimeSignature = ValueObjects.TimeSignature.Common;
        StyleTags = [];
    }

    /// <summary>
    /// Creates a basic specification with minimal parameters.
    /// </summary>
    public static SongSpecification Create(
    string description,
    Genre genre = Genre.Electronic,
    Mood mood = Mood.Chill,
    int tempoBpm = 120,
    int durationSeconds = 180) =>
    new(description, genre, mood, new BpmTempo(tempoBpm), durationSeconds: durationSeconds);
}

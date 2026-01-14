namespace MusicForge.Domain.ValueObjects;

/// <summary>
/// Represents a chord progression with chord names and durations.
/// </summary>
public sealed class ChordProgression
{
    /// <summary>
    /// The chord symbols in the progression (e.g., "Cmaj7", "Dm7", "G7").
    /// </summary>
    public IReadOnlyList<string> Chords { get; }

    /// <summary>
    /// Duration of each chord in beats.
    /// </summary>
    public IReadOnlyList<float> Durations { get; }
    /// Creates a new chord progression.
    /// </summary>
    /// <param name="chords">List of chord symbols.</param>
    /// <param name="durations">Duration of each chord in beats. If null, defaults to 4 beats each.</param>
    [System.Text.Json.Serialization.JsonConstructor]
    public ChordProgression(IReadOnlyList<string> Chords, IReadOnlyList<float>? Durations = null)
    {
        ArgumentNullException.ThrowIfNull(Chords);
        if (Chords.Count == 0)
            throw new ArgumentException("Chord progression must have at least one chord.", nameof(Chords));

        this.Chords = Chords;
        this.Durations = Durations ?? Chords.Select(_ => 4f).ToList();

        if (this.Durations.Count != this.Chords.Count)
            throw new ArgumentException("Durations count must match chords count.", nameof(Durations));
    }

    /// <summary>
    /// Total duration of the progression in beats.
    /// </summary>
    public float TotalBeats => Durations.Sum();

    /// <summary>
    /// Number of chords in the progression.
    /// </summary>
    public int Count => Chords.Count;

    /// <summary>
    /// Creates a progression from a string like "C | Am | F | G" or "Cmaj7 Dm7 G7 Cmaj7".
    /// </summary>
    public static ChordProgression Parse(string input, float defaultDuration = 4f)
    {
        var separator = input.Contains('|') ? '|' : ' ';
        var chords = input.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(s => !string.IsNullOrWhiteSpace(s))
        .ToList();

        return new ChordProgression(chords, chords.Select(_ => defaultDuration).ToList());
    }

    public override string ToString() => string.Join(" | ", Chords);

    public override bool Equals(object? obj) =>
    obj is ChordProgression other && Chords.SequenceEqual(other.Chords) && Durations.SequenceEqual(other.Durations);

    public override int GetHashCode() => HashCode.Combine(Chords.Count, Chords.FirstOrDefault());
}

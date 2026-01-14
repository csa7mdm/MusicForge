namespace MusicForge.Domain.ValueObjects;

/// <summary>
/// Represents a tempo in beats per minute with validation.
/// </summary>
public sealed record class BpmTempo
{
    /// <summary>
    /// The tempo value in beats per minute.
    /// </summary>
    public int Value { get; init; }

    /// <summary>
    /// Minimum allowed tempo.
    /// </summary>
    public const int MinTempo = 40;

    /// <summary>
    /// Maximum allowed tempo.
    /// </summary>
    public const int MaxTempo = 240;

    /// <summary>
    /// Creates a new tempo value with validation.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is outside 40-240 range.</exception>
    public BpmTempo(int value)
    {
        if (value < MinTempo || value > MaxTempo)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value,
            $"Tempo must be between {MinTempo} and {MaxTempo} BPM.");
        }
        Value = value;
    }

    /// <summary>
    /// Implicit conversion from int.
    /// </summary>
    public static implicit operator int(BpmTempo tempo) => tempo.Value;

    /// <summary>
    /// Explicit conversion to BpmTempo.
    /// </summary>
    public static explicit operator BpmTempo(int value) => new(value);

    public override string ToString() => $"{Value} BPM";
}

namespace MusicForge.Domain.ValueObjects;

/// <summary>
/// Represents a musical time signature.
/// </summary>
/// <param name="Numerator">Beats per measure (top number).</param>
/// <param name="Denominator">Note value that gets one beat (bottom number).</param>
public sealed record class TimeSignature(int Numerator, int Denominator)
{
    /// <summary>
    /// Common time (4/4).
    /// </summary>
    public static readonly TimeSignature Common = new(4, 4);

    /// <summary>
    /// Waltz time (3/4).
    /// </summary>
    public static readonly TimeSignature Waltz = new(3, 4);

    /// <summary>
    /// Cut time (2/2).
    /// </summary>
    public static readonly TimeSignature Cut = new(2, 2);

    /// <summary>
    /// 6/8 time signature.
    /// </summary>
    public static readonly TimeSignature SixEight = new(6, 8);

    /// <summary>
    /// Validates that the time signature is musically valid.
    /// </summary>
    public bool IsValid => Numerator > 0 && (Denominator is 2 or 4 or 8 or 16);

    public override string ToString() => $"{Numerator}/{Denominator}";

    /// <summary>
    /// Parses a time signature string like "4/4" or "3/4".
    /// </summary>
    public static TimeSignature Parse(string input)
    {
        var parts = input.Split('/');
        if (parts.Length == 2 && int.TryParse(parts[0], out var num) && int.TryParse(parts[1], out var denom))
        {
            return new TimeSignature(num, denom);
        }
        return Common;
    }
}

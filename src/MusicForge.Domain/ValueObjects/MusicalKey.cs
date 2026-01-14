using MusicForge.Domain.Enums;

namespace MusicForge.Domain.ValueObjects;

/// <summary>
/// Represents a musical key combining a root note and mode.
/// </summary>
/// <param name="Root">The root note of the key.</param>
/// <param name="Mode">The mode (major, minor, etc.) of the key.</param>
public sealed record class MusicalKey(Note Root, Mode Mode)
{
    /// <summary>
    /// Returns string representation like "C Major" or "A Minor".
    /// </summary>
    public override string ToString() => $"{FormatNote(Root)} {Mode}";

    private static string FormatNote(Note note) => note switch
    {
        Note.CSharp => "C#",
        Note.DSharp => "D#",
        Note.FSharp => "F#",
        Note.GSharp => "G#",
        Note.ASharp => "A#",
        _ => note.ToString()
    };

    /// <summary>
    /// Parses a key string like "C Major" or "Am" into a MusicalKey.
    /// </summary>
    public static MusicalKey Parse(string key)
    {
        var normalized = key.Trim().ToLowerInvariant();

        // Handle shorthand like "Am", "Cmaj", etc.
        if (normalized.EndsWith("m") && !normalized.EndsWith("maj"))
        {
            var noteStr = normalized[..^1];
            return new MusicalKey(ParseNote(noteStr), Mode.Minor);
        }

        if (normalized.EndsWith("maj"))
        {
            var noteStr = normalized[..^3];
            return new MusicalKey(ParseNote(noteStr), Mode.Major);
        }

        // Handle "C Major", "A Minor", etc.
        var parts = key.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            var note = ParseNote(parts[0]);
            var mode = Enum.TryParse<Mode>(parts[1], ignoreCase: true, out var m) ? m : Mode.Major;
            return new MusicalKey(note, mode);
        }

        // Default to major if just a note
        return new MusicalKey(ParseNote(normalized), Mode.Major);
    }

    private static Note ParseNote(string noteStr) => noteStr.ToLowerInvariant() switch
    {
        "c" => Note.C,
        "c#" or "csharp" or "db" => Note.CSharp,
        "d" => Note.D,
        "d#" or "dsharp" or "eb" => Note.DSharp,
        "e" => Note.E,
        "f" => Note.F,
        "f#" or "fsharp" or "gb" => Note.FSharp,
        "g" => Note.G,
        "g#" or "gsharp" or "ab" => Note.GSharp,
        "a" => Note.A,
        "a#" or "asharp" or "bb" => Note.ASharp,
        "b" => Note.B,
        _ => Note.C
    };
}

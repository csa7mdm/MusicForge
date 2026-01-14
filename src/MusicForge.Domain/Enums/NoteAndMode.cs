namespace MusicForge.Domain.Enums;

/// <summary>
/// Musical notes in Western music.
/// </summary>
public enum Note
{
    C, CSharp, D, DSharp, E, F, FSharp, G, GSharp, A, ASharp, B
}

/// <summary>
/// Musical modes (major/minor and church modes).
/// </summary>
public enum Mode
{
    /// <summary>Major scale (Ionian mode).</summary>
    Major,

    /// <summary>Natural minor scale (Aeolian mode).</summary>
    Minor,

    /// <summary>Dorian mode - minor with raised 6th.</summary>
    Dorian,

    /// <summary>Phrygian mode - minor with lowered 2nd.</summary>
    Phrygian,

    /// <summary>Lydian mode - major with raised 4th.</summary>
    Lydian,

    /// <summary>Mixolydian mode - major with lowered 7th.</summary>
    Mixolydian
}

namespace MusicForge.Domain.Enums;

/// <summary>
/// Types of audio components in the production pipeline.
/// </summary>
public enum ComponentType
{
    /// <summary>Music theory generation (chords, structure).</summary>
    Theory,

    /// <summary>MIDI data generation.</summary>
    Midi,

    /// <summary>Instrumental audio synthesis.</summary>
    Audio,

    /// <summary>Vocal synthesis from lyrics.</summary>
    Vocals,

    /// <summary>Mixing multiple stems.</summary>
    Mixing,

    /// <summary>Final mastering processing.</summary>
    Mastering,

    /// <summary>Individual audio stems (drums, bass, etc.).</summary>
    Stems
}

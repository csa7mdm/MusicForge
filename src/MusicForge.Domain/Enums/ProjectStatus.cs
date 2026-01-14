namespace MusicForge.Domain.Enums;

/// <summary>
/// Project lifecycle status tracking the generation pipeline.
/// </summary>
public enum ProjectStatus
{
    /// <summary>Initial state, specification defined but generation not started.</summary>
    Draft,

    /// <summary>LLM is composing song structure and theory.</summary>
    Composing,

    /// <summary>Generating MIDI data from composition.</summary>
    GeneratingMidi,

    /// <summary>Synthesizing instrumental audio from MIDI/prompts.</summary>
    SynthesizingAudio,

    /// <summary>Synthesizing vocal audio from lyrics.</summary>
    SynthesizingVocals,

    /// <summary>Mixing all stems together.</summary>
    Mixing,

    /// <summary>Applying final mastering effects.</summary>
    Mastering,

    /// <summary>Generation completed successfully.</summary>
    Complete,

    /// <summary>Generation failed with an error.</summary>
    Failed
}

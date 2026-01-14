using MusicForge.Domain.Abstractions;
using MusicForge.Domain.ValueObjects;

namespace MusicForge.Domain.Entities;

/// <summary>
/// Strongly-typed ID for Arrangement entity.
/// </summary>
public sealed record ArrangementId(Guid Value) : EntityId(Value)
{
    public static ArrangementId New() => new(Guid.NewGuid());
}

/// <summary>
/// Represents the complete arrangement of a song including structure, chords, and MIDI data.
/// </summary>
public sealed class Arrangement : Entity<ArrangementId>
{
    private readonly List<Section> _sections = [];

    /// <summary>
    /// The chord progression for the arrangement.
    /// </summary>
    public ChordProgression ChordProgression { get; private set; }

    /// <summary>
    /// Ordered sections of the arrangement.
    /// </summary>
    public IReadOnlyList<Section> Sections => _sections.AsReadOnly();

    /// <summary>
    /// Total bars in the arrangement.
    /// </summary>
    public int TotalBars { get; private set; }

    /// <summary>
    /// Raw MIDI data for the arrangement.
    /// </summary>
    public byte[]? MidiData { get; private set; }

    private Arrangement() : base()
    {
        ChordProgression = new ChordProgression(["C"]);
    }

    private Arrangement(
    ArrangementId id,
    ChordProgression chordProgression,
    IEnumerable<Section> sections,
    int totalBars,
    byte[]? midiData) : base(id)
    {
        ChordProgression = chordProgression;
        _sections.AddRange(sections);
        TotalBars = totalBars;
        MidiData = midiData;
    }

    /// <summary>
    /// Creates a new arrangement.
    /// </summary>
    public static Arrangement Create(
    ChordProgression chordProgression,
    IEnumerable<Section>? sections = null,
    int totalBars = 64,
    byte[]? midiData = null)
    {
        var sectionList = sections?.ToList() ?? [];
        return new Arrangement(ArrangementId.New(), chordProgression, sectionList, totalBars, midiData);
    }

    /// <summary>
    /// Adds a section to the arrangement.
    /// </summary>
    public void AddSection(Section section)
    {
        _sections.Add(section);
        RecalculateTotalBars();
    }

    /// <summary>
    /// Sets the MIDI data for this arrangement.
    /// </summary>
    public void SetMidiData(byte[] midiData)
    {
        MidiData = midiData;
    }

    private void RecalculateTotalBars()
    {
        if (_sections.Count > 0)
        {
            TotalBars = _sections.Max(s => s.EndBar);
        }
    }
}

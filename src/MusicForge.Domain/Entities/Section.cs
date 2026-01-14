using MusicForge.Domain.Abstractions;

namespace MusicForge.Domain.Entities;

/// <summary>
/// Strongly-typed ID for Section entity.
/// </summary>
public sealed record SectionId(Guid Value) : EntityId(Value)
{
    public static SectionId New() => new(Guid.NewGuid());
}

/// <summary>
/// Represents a section of a song (intro, verse, chorus, etc.).
/// </summary>
public sealed class Section : Entity<SectionId>
{
    /// <summary>
    /// Name of the section (e.g., "intro", "verse1", "chorus").
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Starting bar number (1-indexed).
    /// </summary>
    public int StartBar { get; private set; }

    /// <summary>
    /// Duration of the section in bars.
    /// </summary>
    public int DurationBars { get; private set; }

    /// <summary>
    /// Energy level from 0.0 (quiet) to 1.0 (maximum intensity).
    /// </summary>
    public float EnergyLevel { get; private set; }

    /// <summary>
    /// Elements present in this section (e.g., "drums", "bass", "lead", "pad").
    /// </summary>
    public IReadOnlyList<string> Elements { get; private set; }

    private Section() : base()
    {
        Name = string.Empty;
        Elements = [];
    }

    private Section(
    SectionId id,
    string name,
    int startBar,
    int durationBars,
    float energyLevel,
    IReadOnlyList<string>? elements) : base(id)
    {
        Name = name;
        StartBar = startBar;
        DurationBars = durationBars;
        EnergyLevel = Math.Clamp(energyLevel, 0f, 1f);
        Elements = elements ?? [];
    }

    /// <summary>
    /// Creates a new section.
    /// </summary>
    public static Section Create(
    string name,
    int startBar,
    int durationBars,
    float energyLevel = 0.5f,
    IReadOnlyList<string>? elements = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (startBar < 1) throw new ArgumentOutOfRangeException(nameof(startBar));
        if (durationBars < 1) throw new ArgumentOutOfRangeException(nameof(durationBars));

        return new Section(SectionId.New(), name, startBar, durationBars, energyLevel, elements);
    }

    /// <summary>
    /// End bar (exclusive).
    /// </summary>
    public int EndBar => StartBar + DurationBars;
}

using FluentAssertions;
using MusicForge.Domain.Entities;
using MusicForge.Domain.Enums;
using MusicForge.Domain.Events;
using MusicForge.Domain.ValueObjects;
using Xunit;

namespace MusicForge.Domain.Tests;

public class BpmTempoTests
{
    [Theory]
    [InlineData(40)]
    [InlineData(120)]
    [InlineData(240)]
    public void Create_WithValidValue_Succeeds(int value)
    {
        var tempo = new BpmTempo(value);
        tempo.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(39)]
    [InlineData(241)]
    [InlineData(-1)]
    public void Create_WithInvalidValue_ThrowsArgumentOutOfRangeException(int value)
    {
        var act = () => new BpmTempo(value);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ImplicitConversion_ToInt_Works()
    {
        BpmTempo tempo = new(120);
        int value = tempo;
        value.Should().Be(120);
    }
}

public class MusicalKeyTests
{
    [Fact]
    public void Parse_CMajor_ReturnsCorrectKey()
    {
        var key = MusicalKey.Parse("C Major");
        key.Root.Should().Be(Note.C);
        key.Mode.Should().Be(Mode.Major);
    }

    [Fact]
    public void Parse_Am_ReturnsAMinor()
    {
        var key = MusicalKey.Parse("Am");
        key.Root.Should().Be(Note.A);
        key.Mode.Should().Be(Mode.Minor);
    }

    [Fact]
    public void Parse_FSharpMinor_ReturnsCorrectKey()
    {
        var key = MusicalKey.Parse("F# Minor");
        key.Root.Should().Be(Note.FSharp);
        key.Mode.Should().Be(Mode.Minor);
    }

    [Fact]
    public void ToString_ReturnsFormattedKey()
    {
        var key = new MusicalKey(Note.CSharp, Mode.Minor);
        key.ToString().Should().Be("C# Minor");
    }
}

public class ChordProgressionTests
{
    [Fact]
    public void Parse_WithPipes_SplitsCorrectly()
    {
        var progression = ChordProgression.Parse("C | Am | F | G");
        progression.Chords.Should().HaveCount(4);
        progression.Chords[0].Should().Be("C");
        progression.Chords[3].Should().Be("G");
    }

    [Fact]
    public void Parse_WithSpaces_SplitsCorrectly()
    {
        var progression = ChordProgression.Parse("Cmaj7 Dm7 G7 Cmaj7");
        progression.Chords.Should().HaveCount(4);
    }

    [Fact]
    public void TotalBeats_CalculatesCorrectly()
    {
        var progression = new ChordProgression(["C", "G", "Am", "F"], [4f, 4f, 4f, 4f]);
        progression.TotalBeats.Should().Be(16f);
    }

    [Fact]
    public void Create_WithMismatchedDurations_Throws()
    {
        var act = () => new ChordProgression(["C", "G"], [4f]);
        act.Should().Throw<ArgumentException>();
    }
}

public class SongSpecificationTests
{
    [Fact]
    public void Create_WithMinimalParams_UsesSensibleDefaults()
    {
        var spec = SongSpecification.Create("test description");

        spec.Genre.Should().Be(Genre.Electronic);
        spec.Mood.Should().Be(Mood.Chill);
        spec.Tempo.Value.Should().Be(120);
        spec.DurationSeconds.Should().Be(180);
        spec.HasVocals.Should().BeFalse();
    }

    [Fact]
    public void Create_WithEmptyDescription_Throws()
    {
        var act = () => SongSpecification.Create("");
        act.Should().Throw<ArgumentException>();
    }
}

public class ProjectTests
{
    [Fact]
    public void Create_WithValidSpec_ReturnsProject()
    {
        var spec = SongSpecification.Create("test track", Genre.Electronic, Mood.Energetic, 128);
        var project = Project.Create("Test Project", spec);

        project.Should().NotBeNull();
        project.Status.Should().Be(ProjectStatus.Draft);
        project.Context.Should().NotBeNull();
        project.DomainEvents.Should().ContainSingle(e => e is ProjectCreatedEvent);
    }

    [Fact]
    public void StartGeneration_TransitionsToComposing()
    {
        var project = CreateTestProject();
        project.StartGeneration();

        project.Status.Should().Be(ProjectStatus.Composing);
        project.Context.FocusCheckpoints.Should().NotBeEmpty();
        project.DomainEvents.Should().Contain(e => e is GenerationStartedEvent);
    }

    [Fact]
    public void StartGeneration_WhenNotDraft_Throws()
    {
        var project = CreateTestProject();
        project.StartGeneration();

        var act = () => project.StartGeneration();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddStem_AddsStemToCollection()
    {
        var project = CreateTestProject();
        var stem = Stem.Create("drums", ComponentType.Stems, "/path/to/drums.wav");

        project.AddStem(stem);

        project.Stems.Should().Contain(stem);
        project.DomainEvents.Should().Contain(e => e is StemCompletedEvent);
    }

    [Fact]
    public void Complete_WithMasterFile_Succeeds()
    {
        var project = CreateTestProject();
        project.StartGeneration();
        project.SetMasterFile("/path/to/master.wav");

        project.Complete();

        project.Status.Should().Be(ProjectStatus.Complete);
        project.DomainEvents.Should().Contain(e => e is ProjectCompletedEvent);
    }

    [Fact]
    public void Complete_WithoutMasterFile_Throws()
    {
        var project = CreateTestProject();
        project.StartGeneration();

        var act = () => project.Complete();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Fail_SetsReasonAndStatus()
    {
        var project = CreateTestProject();
        project.StartGeneration();

        project.Fail("Test failure reason");

        project.Status.Should().Be(ProjectStatus.Failed);
        project.FailureReason.Should().Be("Test failure reason");
        project.DomainEvents.Should().Contain(e => e is ProjectFailedEvent);
    }

    private static Project CreateTestProject()
    {
        var spec = SongSpecification.Create("test track", Genre.LoFi, Mood.Chill, 85, 120);
        return Project.Create("Test Project", spec);
    }
}

public class SectionTests
{
    [Fact]
    public void Create_WithValidParams_Succeeds()
    {
        var section = Section.Create("verse1", 1, 8, 0.5f, ["drums", "bass"]);

        section.Name.Should().Be("verse1");
        section.StartBar.Should().Be(1);
        section.DurationBars.Should().Be(8);
        section.EndBar.Should().Be(9);
        section.EnergyLevel.Should().Be(0.5f);
        section.Elements.Should().HaveCount(2);
    }

    [Fact]
    public void Create_WithEnergyOutOfRange_ClampesToValid()
    {
        var section = Section.Create("chorus", 1, 8, 1.5f);
        section.EnergyLevel.Should().Be(1f);
    }
}

public class AgentContextTests
{
    [Fact]
    public void AddFocusCheckpoint_KeepsLast10()
    {
        var context = new AgentContext();

        for (int i = 0; i < 15; i++)
        {
            context.AddFocusCheckpoint($"Checkpoint {i}");
        }

        context.FocusCheckpoints.Should().HaveCount(10);
        context.FocusCheckpoints.First().Should().Contain("5");
    }

    [Fact]
    public void ToPromptContext_ReturnsFormattedString()
    {
        var context = new AgentContext();
        context.EstablishMusicalContext(
        new MusicalKey(Note.C, Mode.Major),
        new BpmTempo(120),
        Genre.Electronic);

        var prompt = context.ToPromptContext();

        prompt.Should().Contain("Genre: Electronic");
        prompt.Should().Contain("Key: C Major");
        prompt.Should().Contain("Tempo: 120 BPM");
    }
}

using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using MusicForge.Application.Interfaces;
using MusicForge.Domain.Entities;
using MusicForge.Domain.Enums;
using MusicForge.Domain.ValueObjects;

namespace MusicForge.Cli.Commands;

/// <summary>
/// Settings for the new command.
/// </summary>
public sealed class NewCommandSettings : CommandSettings
{
    [CommandArgument(0, "[NAME]")]
    [Description("Project name")]
    public string? Name { get; set; }

    [CommandOption("-g|--genre <GENRE>")]
    [Description("Music genre")]
    public string? Genre { get; set; }

    [CommandOption("-m|--mood <MOOD>")]
    [Description("Emotional mood")]
    public string? Mood { get; set; }

    [CommandOption("-t|--tempo <BPM>")]
    [Description("Tempo in BPM (40-240)")]
    [DefaultValue(120)]
    public int Tempo { get; set; } = 120;

    [CommandOption("-k|--key <KEY>")]
    [Description("Musical key (e.g., 'C Major', 'Am')")]
    [DefaultValue("C Major")]
    public string Key { get; set; } = "C Major";

    [CommandOption("-d|--duration <SECONDS>")]
    [Description("Duration in seconds")]
    [DefaultValue(180)]
    public int Duration { get; set; } = 180;

    [CommandOption("--vocals")]
    [Description("Include vocals")]
    public bool HasVocals { get; set; }

    [CommandOption("-i|--interactive")]
    [Description("Interactive mode")]
    public bool Interactive { get; set; }
}

/// <summary>
/// Command to create a new music project.
/// </summary>
public sealed class NewCommand(IProjectRepository repository) : AsyncCommand<NewCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, NewCommandSettings settings, CancellationToken ct)
    {
        AnsiConsole.Write(new FigletText("MusicForge").Color(Color.Purple));

        string name;
        Genre genre;
        Mood mood;
        int tempo;
        string key;
        int duration;
        bool hasVocals;

        if (settings.Interactive || string.IsNullOrEmpty(settings.Name))
        {
            // Interactive mode
            name = AnsiConsole.Ask<string>("Project [green]name[/]:");

            genre = AnsiConsole.Prompt(
            new SelectionPrompt<Genre>()
            .Title("Select [green]genre[/]:")
            .AddChoices(Enum.GetValues<Genre>()));

            mood = AnsiConsole.Prompt(
            new SelectionPrompt<Mood>()
            .Title("Select [green]mood[/]:")
            .AddChoices(Enum.GetValues<Mood>()));

            tempo = AnsiConsole.Prompt(
            new TextPrompt<int>("Enter [green]tempo[/] (BPM):")
            .DefaultValue(120)
            .Validate(t => t is >= 40 and <= 240
            ? ValidationResult.Success()
            : ValidationResult.Error("Tempo must be between 40 and 240")));

            key = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("Select [green]key[/]:")
            .AddChoices("C Major", "G Major", "D Major", "A Major", "E Major",
            "A Minor", "E Minor", "D Minor", "G Minor", "F Minor"));

            duration = AnsiConsole.Prompt(
            new TextPrompt<int>("Enter [green]duration[/] (seconds):")
            .DefaultValue(180)
            .Validate(d => d is > 0 and <= 600
            ? ValidationResult.Success()
            : ValidationResult.Error("Duration must be 1-600 seconds")));

            hasVocals = AnsiConsole.Confirm("Include [green]vocals[/]?", false);
        }
        else
        {
            name = settings.Name;
            genre = Enum.TryParse<Genre>(settings.Genre, true, out var g) ? g : Genre.Electronic;
            mood = Enum.TryParse<Mood>(settings.Mood, true, out var m) ? m : Mood.Chill;
            tempo = settings.Tempo;
            key = settings.Key;
            duration = settings.Duration;
            hasVocals = settings.HasVocals;
        }

        // Create project
        var spec = new SongSpecification(
        description: $"{name} - {genre} track",
        genre: genre,
        mood: mood,
        tempo: new BpmTempo(tempo),
        key: MusicalKey.Parse(key),
        durationSeconds: duration,
        hasVocals: hasVocals
        );

        var project = Project.Create(name, spec);

        await AnsiConsole.Status()
        .StartAsync("Creating project...", async ctx =>
        {
            await repository.SaveAsync(project, ct);
            await Task.Delay(500, ct); // Visual feedback
        });

        AnsiConsole.MarkupLine($"\n[green]✓[/] Created project [bold]{name}[/]");
        AnsiConsole.MarkupLine($"  ID: [dim]{project.Id.Value}[/]");
        AnsiConsole.MarkupLine($"  Genre: [cyan]{genre}[/] | Mood: [cyan]{mood}[/]");
        AnsiConsole.MarkupLine($"  Key: [cyan]{key}[/] | Tempo: [cyan]{tempo} BPM[/]");
        AnsiConsole.MarkupLine($"  Duration: [cyan]{duration}s[/] | Vocals: [cyan]{(hasVocals ? "Yes" : "No")}[/]");
        AnsiConsole.MarkupLine($"\nRun [yellow]musicforge generate {project.Id.Value}[/] to start generation.");

        return 0;
    }
}

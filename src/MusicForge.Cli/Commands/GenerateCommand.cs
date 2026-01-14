using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using MusicForge.Application.Interfaces;
using MusicForge.Domain.Entities;

namespace MusicForge.Cli.Commands;

/// <summary>
/// Settings for the generate command.
/// </summary>
public sealed class GenerateCommandSettings : CommandSettings
{
    [CommandArgument(0, "<PROJECT_ID>")]
    [Description("Project ID to generate")]
    public required string ProjectId { get; set; }

    [CommandOption("-p|--prompt <PROMPT>")]
    [Description("Additional generation prompt")]
    public string? Prompt { get; set; }

    [CommandOption("--no-progress")]
    [Description("Disable progress display")]
    public bool NoProgress { get; set; }
}

/// <summary>
/// Command to generate music for a project.
/// </summary>
public sealed class GenerateCommand(IProjectRepository repository) : AsyncCommand<GenerateCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, GenerateCommandSettings settings, CancellationToken ct)
    {
        if (!Guid.TryParse(settings.ProjectId, out var projectGuid))
        {
            AnsiConsole.MarkupLine("[red]Invalid project ID format.[/]");
            return 1;
        }

        var projectId = new ProjectId(projectGuid);
        var project = await repository.GetByIdAsync(projectId, ct);

        if (project is null)
        {
            AnsiConsole.MarkupLine($"[red]Project not found:[/] {settings.ProjectId}");
            return 1;
        }

        AnsiConsole.MarkupLine($"[bold]Generating:[/] {project.Name}");
        AnsiConsole.MarkupLine($"[dim]Genre:[/] {project.Specification.Genre} | [dim]Mood:[/] {project.Specification.Mood}");
        AnsiConsole.MarkupLine($"[dim]Key:[/] {project.Specification.Key} | [dim]Tempo:[/] {project.Specification.Tempo.Value} BPM");
        AnsiConsole.WriteLine();

        // Simulate generation with progress
        var stages = new[]
        {
("Analyzing request", 0.1),
("Generating chord progression", 0.2),
("Composing arrangement", 0.35),
("Synthesizing drums", 0.5),
("Synthesizing bass", 0.65),
("Synthesizing melodies", 0.8),
("Mixing stems", 0.9),
("Mastering", 1.0)
};

        if (!settings.NoProgress)
        {
            await AnsiConsole.Progress()
            .Columns(
            new TaskDescriptionColumn(),
            new ProgressBarColumn(),
            new PercentageColumn(),
            new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask("[green]Generating music[/]");

                foreach (var (stage, progress) in stages)
                {
                    task.Description = $"[cyan]{stage}[/]";
                    var targetProgress = progress * 100;

                    while (task.Value < targetProgress)
                    {
                        task.Increment(1);
                        await Task.Delay(50, ct);
                    }
                }
            });
        }
        else
        {
            foreach (var (stage, _) in stages)
            {
                AnsiConsole.MarkupLine($"[dim]→[/] {stage}...");
                await Task.Delay(200, ct);
            }
        }

        // Mark project as complete (simulation)
        project.StartGeneration();

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[green]Generation Complete[/]").RuleStyle("green"));
        AnsiConsole.MarkupLine($"\n[green]✓[/] Generated: [bold]{project.Name}[/]");
        AnsiConsole.MarkupLine($"  [dim]Duration:[/] {project.Specification.DurationSeconds}s");
        AnsiConsole.MarkupLine($"\n  Run [yellow]musicforge export {project.Id.Value}[/] to export.");

        return 0;
    }
}

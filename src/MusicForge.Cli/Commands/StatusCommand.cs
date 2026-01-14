using System.ComponentModel;
using MusicForge.Application.Interfaces;
using MusicForge.Domain.Entities;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MusicForge.Cli.Commands;

/// <summary>
/// Settings for the status command.
/// </summary>
public sealed class StatusCommandSettings : CommandSettings
{
    [CommandArgument(0, "<PROJECT_ID>")]
    [Description("Project ID to show status")]
    public required string ProjectId { get; set; }
}

/// <summary>
/// Command to display project status and details.
/// </summary>
public sealed class StatusCommand(IProjectRepository repository) : AsyncCommand<StatusCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, StatusCommandSettings settings, CancellationToken ct)
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

        // Header
        var statusColor = project.Status switch
        {
            Domain.Enums.ProjectStatus.Complete => "green",
            Domain.Enums.ProjectStatus.Failed => "red",
            Domain.Enums.ProjectStatus.Draft => "dim",
            _ => "yellow"
        };

        AnsiConsole.Write(new Rule($"[bold]{project.Name}[/]").RuleStyle("cyan"));
        AnsiConsole.MarkupLine($"[dim]ID:[/] {project.Id.Value}");
        AnsiConsole.MarkupLine($"[dim]Status:[/] [{statusColor}]{project.Status}[/]");
        AnsiConsole.WriteLine();

        // Specification panel
        var specTable = new Table().Border(TableBorder.None).HideHeaders();
        specTable.AddColumn("Key");
        specTable.AddColumn("Value");
        specTable.AddRow("[dim]Description[/]", project.Specification.Description);
        specTable.AddRow("[dim]Genre[/]", project.Specification.Genre.ToString());
        specTable.AddRow("[dim]Mood[/]", project.Specification.Mood.ToString());
        specTable.AddRow("[dim]Key[/]", project.Specification.Key.ToString());
        specTable.AddRow("[dim]Tempo[/]", $"{project.Specification.Tempo.Value} BPM");
        specTable.AddRow("[dim]Duration[/]", $"{project.Specification.DurationSeconds} seconds");
        specTable.AddRow("[dim]Vocals[/]", project.Specification.HasVocals ? "Yes" : "No");

        var specPanel = new Panel(specTable)
        .Header("[cyan]Specification[/]")
        .Border(BoxBorder.Rounded);
        AnsiConsole.Write(specPanel);

        // Stems
        if (project.Stems.Any())
        {
            var stemTable = new Table()
            .Border(TableBorder.Simple)
            .AddColumn("Stem")
            .AddColumn("Duration")
            .AddColumn("File");

            foreach (var stem in project.Stems)
            {
                stemTable.AddRow(
                stem.Name,
                stem.Duration.ToString(@"mm\:ss"),
                stem.Path ?? "[dim]pending[/]"
                );
            }

            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Panel(stemTable)
            .Header("[cyan]Stems[/]")
            .Border(BoxBorder.Rounded));
        }

        // Timestamps
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[dim]Created:[/] {project.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        AnsiConsole.MarkupLine($"[dim]Updated:[/] {project.UpdatedAt:yyyy-MM-dd HH:mm:ss}");

        return 0;
    }
}

using Spectre.Console;
using Spectre.Console.Cli;
using MusicForge.Application.Interfaces;

namespace MusicForge.Cli.Commands;

/// <summary>
/// Settings for list command.
/// </summary>
public sealed class ListCommandSettings : CommandSettings
{
    [CommandOption("--json")]
    [System.ComponentModel.Description("Output as JSON")]
    public bool Json { get; set; }
}

/// <summary>
/// Command to list all projects.
/// </summary>
public sealed class ListCommand(IProjectRepository repository) : AsyncCommand<ListCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ListCommandSettings settings, CancellationToken ct)
    {
        var projects = await repository.GetAllAsync(ct);

        if (projects.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No projects found.[/]");
            AnsiConsole.MarkupLine("Run [cyan]musicforge new[/] to create one.");
            return 0;
        }

        if (settings.Json)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(
            projects.Select(p => new
            {
                Id = p.Id.Value,
                p.Name,
                Status = p.Status.ToString(),
                Genre = p.Specification.Genre.ToString(),
                Tempo = p.Specification.Tempo.Value,
                p.CreatedAt,
                p.UpdatedAt
            }),
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
            return 0;
        }

        var table = new Table()
        .Border(TableBorder.Rounded)
        .AddColumn("[bold]ID[/]")
        .AddColumn("[bold]Name[/]")
        .AddColumn("[bold]Status[/]")
        .AddColumn("[bold]Genre[/]")
        .AddColumn("[bold]Tempo[/]")
        .AddColumn("[bold]Created[/]");

        foreach (var project in projects.OrderByDescending(p => p.CreatedAt))
        {
            var statusColor = project.Status switch
            {
                Domain.Enums.ProjectStatus.Complete => "green",
                Domain.Enums.ProjectStatus.Failed => "red",
                Domain.Enums.ProjectStatus.Draft => "dim",
                _ => "yellow"
            };

            table.AddRow(
            $"[dim]{project.Id.Value.ToString()[..8]}...[/]",
            project.Name,
            $"[{statusColor}]{project.Status}[/]",
            project.Specification.Genre.ToString(),
            $"{project.Specification.Tempo.Value} BPM",
            project.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[dim]Total: {projects.Count} project(s)[/]");

        return 0;
    }
}

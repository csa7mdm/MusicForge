using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MusicForge.Api.DTOs;
using MusicForge.Application.Commands;
using MusicForge.Application.Interfaces;
using MusicForge.Domain.Entities;
using MusicForge.Domain.Enums;

namespace MusicForge.Api.Endpoints;

/// <summary>
/// Project management endpoints.
/// </summary>
public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects")
        .WithTags("Projects");

        group.MapGet("/", GetAllProjects)
        .WithName("GetProjects")
        .WithSummary("List all projects");

        group.MapGet("/{id:guid}", GetProject)
        .WithName("GetProject")
        .WithSummary("Get project details");

        group.MapPost("/", CreateProject)
        .WithName("CreateProject")
        .WithSummary("Create a new music project");

        group.MapPost("/{id:guid}/generate", GenerateSong)
        .WithName("GenerateSong")
        .WithSummary("Start music generation");

        group.MapPost("/{id:guid}/iterate", IterateOnFeedback)
        .WithName("IterateOnFeedback")
        .WithSummary("Iterate with feedback");

        group.MapDelete("/{id:guid}", DeleteProject)
        .WithName("DeleteProject")
        .WithSummary("Delete a project");
    }

    private static async Task<Results<Ok<IReadOnlyList<ProjectSummary>>, ProblemHttpResult>> GetAllProjects(
    IProjectRepository repository,
    CancellationToken ct)
    {
        var projects = await repository.GetAllAsync(ct);

        var summaries = projects.Select(p => new ProjectSummary(
        p.Id.Value,
        p.Name,
        p.Status.ToString(),
        p.Specification.Genre.ToString(),
        p.Specification.Tempo.Value,
        p.Specification.DurationSeconds,
        p.CreatedAt,
        p.UpdatedAt
        )).ToList();

        return TypedResults.Ok<IReadOnlyList<ProjectSummary>>(summaries);
    }

    private static async Task<Results<Ok<ProjectDetails>, NotFound>> GetProject(
    Guid id,
    IProjectRepository repository,
    CancellationToken ct)
    {
        var project = await repository.GetByIdAsync(new ProjectId(id), ct);
        if (project is null)
            return TypedResults.NotFound();

        var details = MapToDetails(project);
        return TypedResults.Ok(details);
    }

    private static async Task<Results<Created<CreateProjectResponse>, BadRequest<string>>> CreateProject(
    CreateProjectRequest request,
    IMediator mediator,
    CancellationToken ct)
    {
        if (!Enum.TryParse<Genre>(request.Genre, true, out var genre))
            return TypedResults.BadRequest($"Invalid genre: {request.Genre}");

        if (!Enum.TryParse<Mood>(request.Mood, true, out var mood))
            return TypedResults.BadRequest($"Invalid mood: {request.Mood}");

        var command = new CreateProjectCommand(
        request.Name,
        request.Description,
        genre,
        mood,
        request.TempoBpm,
        request.Key,
        request.DurationSeconds,
        request.HasVocals,
        request.Lyrics
        );

        var projectId = await mediator.Send(command, ct);

        var response = new CreateProjectResponse(projectId.Value, "Project created successfully");
        return TypedResults.Created($"/api/projects/{projectId.Value}", response);
    }

    private static async Task<Results<Ok<GenerationResponse>, NotFound, BadRequest<string>>> GenerateSong(
    Guid id,
    GenerateRequest request,
    IMediator mediator,
    CancellationToken ct)
    {
        var command = new GenerateSongCommand(new ProjectId(id), request.Prompt);
        var result = await mediator.Send(command, ct);

        if (!result.Success && result.ErrorMessage?.Contains("not found") == true)
            return TypedResults.NotFound();

        return TypedResults.Ok(new GenerationResponse(
        result.Success,
        result.ErrorMessage,
        result.MasterFilePath,
        result.StemPaths
        ));
    }

    private static async Task<Results<Ok<GenerationResponse>, NotFound>> IterateOnFeedback(
    Guid id,
    IterateRequest request,
    IMediator mediator,
    CancellationToken ct)
    {
        var command = new IterateOnFeedbackCommand(new ProjectId(id), request.Feedback, request.TargetSection);
        var result = await mediator.Send(command, ct);

        if (!result.Success && result.ErrorMessage?.Contains("not found") == true)
            return TypedResults.NotFound();

        return TypedResults.Ok(new GenerationResponse(
        result.Success,
        result.ErrorMessage,
        result.MasterFilePath,
        result.StemPaths
        ));
    }

    private static async Task<Results<NoContent, NotFound>> DeleteProject(
    Guid id,
    IProjectRepository repository,
    CancellationToken ct)
    {
        var project = await repository.GetByIdAsync(new ProjectId(id), ct);
        if (project is null)
            return TypedResults.NotFound();

        await repository.DeleteAsync(project.Id, ct);
        return TypedResults.NoContent();
    }

    private static ProjectDetails MapToDetails(Project project)
    {
        return new ProjectDetails(
        project.Id.Value,
        project.Name,
        project.Specification.Description,
        project.Status.ToString(),
        project.Specification.Genre.ToString(),
        project.Specification.Mood.ToString(),
        project.Specification.Tempo.Value,
        project.Specification.Key.ToString(),
        project.Specification.DurationSeconds,
        project.Specification.HasVocals,
        project.Specification.Lyrics,
        project.MasterFilePath,
        project.Stems.Select(s => new StemInfo(s.Name, s.Path, s.Duration)).ToList(),
        project.Arrangement is not null ? new ArrangementInfo(
        project.Arrangement.ChordProgression.Chords.ToList(),
        project.Arrangement.TotalBars,
        project.Arrangement.Sections.Select(s => new SectionInfo(
            s.Name, s.StartBar, s.DurationBars, s.EnergyLevel, s.Elements.ToList())).ToList()
        ) : null,
        project.CreatedAt,
        project.UpdatedAt
        );
    }
}

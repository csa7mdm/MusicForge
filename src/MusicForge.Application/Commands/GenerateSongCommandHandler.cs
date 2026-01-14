using MediatR;
using MusicForge.Application.Interfaces;

namespace MusicForge.Application.Commands;

/// <summary>
/// Handler for GenerateSongCommand.
/// </summary>
public sealed class GenerateSongCommandHandler(
IProjectRepository repository,
IAgentOrchestrator orchestrator)
: IRequestHandler<GenerateSongCommand, GenerationResult>
{
    public async Task<GenerationResult> Handle(GenerateSongCommand request, CancellationToken cancellationToken)
    {
        var project = await repository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            return new GenerationResult(
            Success: false,
            ErrorMessage: $"Project {request.ProjectId} not found.",
            MasterFilePath: null,
            StemPaths: []
            );
        }

        try
        {
            var result = await orchestrator.GenerateAsync(project, request.Prompt, cancellationToken);
            await repository.SaveAsync(project, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            project.Fail(ex.Message);
            await repository.SaveAsync(project, cancellationToken);

            return new GenerationResult(
            Success: false,
            ErrorMessage: ex.Message,
            MasterFilePath: null,
            StemPaths: []
            );
        }
    }
}

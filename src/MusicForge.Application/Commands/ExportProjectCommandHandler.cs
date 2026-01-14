using MediatR;
using MusicForge.Application.Interfaces;

namespace MusicForge.Application.Commands;

public sealed class ExportProjectCommandHandler(IProjectRepository repository)
    : IRequestHandler<ExportProjectCommand, ExportResult>
{
    public async Task<ExportResult> Handle(ExportProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await repository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            return new ExportResult(
                Success: false,
                ErrorMessage: $"Project {request.ProjectId} not found.",
                ExportPath: string.Empty,
                FileSizeBytes: 0
            );
        }

        // TODO: Implement actual export logic via an IProjectExporter service
        // For now, valid placeholder logic

        await Task.Yield(); // Simulate work

        return new ExportResult(
            Success: false,
            ErrorMessage: "Export service not yet implemented.",
            ExportPath: string.Empty,
            FileSizeBytes: 0
        );
    }
}

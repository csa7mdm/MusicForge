using MediatR;
using MusicForge.Application.Interfaces;
using MusicForge.Domain.Entities;
using MusicForge.Domain.ValueObjects;

namespace MusicForge.Application.Commands;

/// <summary>
/// Handler for CreateProjectCommand.
/// </summary>
public sealed class CreateProjectCommandHandler(IProjectRepository repository)
: IRequestHandler<CreateProjectCommand, ProjectId>
{
    public async Task<ProjectId> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var key = MusicalKey.Parse(request.Key);
        var tempo = new BpmTempo(request.TempoBpm);

        var specification = new SongSpecification(
        description: request.Description,
        genre: request.Genre,
        mood: request.Mood,
        tempo: tempo,
        key: key,
        durationSeconds: request.DurationSeconds,
        hasVocals: request.HasVocals,
        lyrics: request.Lyrics
        );

        var project = Project.Create(request.Name, specification);

        await repository.SaveAsync(project, cancellationToken);

        return project.Id;
    }
}

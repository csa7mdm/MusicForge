using System.Collections.Concurrent;
using MusicForge.Application.Interfaces;
using MusicForge.Domain.Entities;

namespace MusicForge.Infrastructure.Persistence;

/// <summary>
/// In-memory repository for development and testing.
/// </summary>
public sealed class InMemoryProjectRepository : IProjectRepository
{
    private readonly ConcurrentDictionary<ProjectId, Project> _projects = new();

    public Task<Project?> GetByIdAsync(ProjectId id, CancellationToken ct = default)
    {
        _projects.TryGetValue(id, out var project);
        return Task.FromResult(project);
    }

    public Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<Project> projects = _projects.Values.ToList();
        return Task.FromResult(projects);
    }

    public Task SaveAsync(Project project, CancellationToken ct = default)
    {
        _projects[project.Id] = project;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ProjectId id, CancellationToken ct = default)
    {
        _projects.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}

using Microsoft.EntityFrameworkCore;
using MusicForge.Application.Interfaces;
using MusicForge.Domain.Entities;

namespace MusicForge.Infrastructure.Persistence;

public class SqliteProjectRepository(MusicForgeDbContext dbContext) : IProjectRepository
{
    public async Task<Project?> GetByIdAsync(ProjectId id, CancellationToken ct = default)
    {
        return await dbContext.Projects
            .Include(p => p.Stems)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken ct = default)
    {
        return await dbContext.Projects
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task SaveAsync(Project project, CancellationToken ct = default)
    {
        var existing = await dbContext.Projects.FindAsync([project.Id], ct);
        if (existing is null)
        {
            await dbContext.Projects.AddAsync(project, ct);
        }
        else
        {
            // If tracked, EF Core detects changes.
            // If we are passing a detached entity here, we might need to update it.
            // But FindAsync attaches it if found?
            // If 'project' object reference is different from 'existing', we have an issue.
            // Usually in DDD repository pattern, we assume the object passed is the one being worked on.
            // But if we retrieved it via AsNoTracking or different context scope...
            // Let's assume standard scoped lifetime usage: Retrieve -> Modify -> Save.
            // In that case, 'project' should be already attached if retrieved from same context.
            // If 'project' ID matches 'existing' ID but references differ, we'd need to update values.
            
            // However, GetByIdAsync didn't use AsNoTracking (except for stems include?).
            // Just ensuring it's attached.
            if (dbContext.Entry(project).State == EntityState.Detached)
            {
               dbContext.Update(project);
            }
        }

        await dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(ProjectId id, CancellationToken ct = default)
    {
        var project = await dbContext.Projects.FindAsync([id], ct);
        if (project is not null)
        {
            dbContext.Projects.Remove(project);
            await dbContext.SaveChangesAsync(ct);
        }
    }
}

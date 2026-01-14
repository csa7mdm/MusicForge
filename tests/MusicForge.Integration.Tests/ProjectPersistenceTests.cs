using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MusicForge.Domain.Entities;
using MusicForge.Domain.Enums;
using MusicForge.Domain.ValueObjects;
using MusicForge.Infrastructure.Persistence;

namespace MusicForge.Integration.Tests;

public class ProjectPersistenceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly MusicForgeDbContext _dbContext;
    private readonly SqliteProjectRepository _repository;

    public ProjectPersistenceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<MusicForgeDbContext>()
            .UseSqlite(_connection)
            .EnableServiceProviderCaching(false)
            .Options;

        _dbContext = new MusicForgeDbContext(options);
        // EnsureCreated needs to run to setup schema
        try
        {
            _dbContext.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            // Log or inspect ex if possible, though test runner handles it
            throw new Exception($"EnsureCreated failed: {ex.Message}", ex);
        }

        _repository = new SqliteProjectRepository(_dbContext);
    }

    [Fact]
    public async Task SaveValues_AndRetrieve_ShouldMatch()
    {
        // Arrange
        var spec = SongSpecification.Create("Integration Test Track", Genre.Electronic, Mood.Energetic, 128);
        var project = Project.Create("Test Project", spec);

        project.StartGeneration();
        project.Context.AddFocusCheckpoint("Initial checkpoint");
        project.Context.EstablishMusicalContext(new MusicalKey(Note.C, Mode.Major), new BpmTempo(128), Genre.Electronic);

        // Act
        await _repository.SaveAsync(project);

        // Clear tracker to ensure we read from DB
        _dbContext.ChangeTracker.Clear();

        var retrieved = await _repository.GetByIdAsync(project.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(project.Id);
        retrieved.Name.Should().Be(project.Name);
        retrieved.Status.Should().Be(project.Status);

        // Value Objects
        retrieved.Specification.Genre.Should().Be(Genre.Electronic);
        retrieved.Specification.Tempo.Value.Should().Be(128);

        // Complex Types (JSON)
        retrieved.Context.FocusCheckpoints.Should().Contain("Initial checkpoint");
        retrieved.Context.EstablishedKey.Should().Be(new MusicalKey(Note.C, Mode.Major));
    }

    public void Dispose()
    {
        _connection.Dispose();
        _dbContext.Dispose();
    }
}

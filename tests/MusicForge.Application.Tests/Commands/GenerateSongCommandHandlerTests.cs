using FluentAssertions;
using Moq;
using MusicForge.Application.Commands;
using MusicForge.Application.Interfaces;
using MusicForge.Domain.Entities;
using MusicForge.Domain.Enums;
using MusicForge.Domain.ValueObjects; // Added for SongSpecification
using Xunit;

namespace MusicForge.Application.Tests.Commands;

public class GenerateSongCommandHandlerTests
{
    private readonly Mock<IProjectRepository> _mockRepository;
    private readonly Mock<IAgentOrchestrator> _mockOrchestrator;
    private readonly GenerateSongCommandHandler _handler;

    public GenerateSongCommandHandlerTests()
    {
        _mockRepository = new Mock<IProjectRepository>();
        _mockOrchestrator = new Mock<IAgentOrchestrator>();
        _handler = new GenerateSongCommandHandler(_mockRepository.Object, _mockOrchestrator.Object);
    }

    [Fact]
    public async Task Handle_ShouldGenerateSong_WhenProjectExists()
    {
        // Arrange
        var projectId = new ProjectId(Guid.NewGuid());
        var command = new GenerateSongCommand(projectId, "Make it upbeat");

        // Define minimal spec for Project.Create
        var spec = new SongSpecification(
             description: "Test",
             genre: Genre.Electronic,
             mood: Mood.Energetic,
             tempo: new BpmTempo(120),
             key: MusicalKey.Parse("C Minor"),
             durationSeconds: 180,
             hasVocals: false,
             lyrics: null
         );

        // We need to create a project with specific ID, but factory creates new ID.
        // Assuming GetByIdAsync returns a project we can use.
        // We can't set ID of project easily without reflection or altering design, 
        // but for test we just need *a* project returned by repo.
        var project = Project.Create("Test Project", spec);
        // Force the ID to match if needed, or better, just have repo return this project when asked for *any* ID or the specific command ID

        _mockRepository.Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var expectedResult = new GenerationResult(true, null, "master.wav", new List<string>());
        _mockOrchestrator.Setup(o => o.GenerateAsync(project, "Make it upbeat", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _mockRepository.Verify(r => r.SaveAsync(project, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenProjectNotFound()
    {
        // Arrange
        var projectId = new ProjectId(Guid.NewGuid());
        var command = new GenerateSongCommand(projectId, "Make it upbeat");

        _mockRepository.Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
        _mockOrchestrator.Verify(o => o.GenerateAsync(It.IsAny<Project>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

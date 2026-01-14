using FluentAssertions;
using Moq;
using MusicForge.Application.Commands;
using MusicForge.Application.Interfaces;
using MusicForge.Domain.Entities;
using MusicForge.Domain.Enums;
using Xunit;

namespace MusicForge.Application.Tests.Commands;

public class CreateProjectCommandHandlerTests
{
    private readonly Mock<IProjectRepository> _mockRepository;
    private readonly CreateProjectCommandHandler _handler;

    public CreateProjectCommandHandlerTests()
    {
        _mockRepository = new Mock<IProjectRepository>();
        _handler = new CreateProjectCommandHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateAndSaveProject()
    {
        // Arrange
        var command = new CreateProjectCommand(
            Name: "Test Project",
            Description: "A test project",
            Genre: Genre.Electronic,
            Mood: Mood.Energetic,
            TempoBpm: 120,
            Key: "C Minor",
            DurationSeconds: 180,
            HasVocals: false,
            Lyrics: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.SaveAsync(It.Is<Project>(p => 
            p.Name == "Test Project" && 
            p.Specification.Genre == Genre.Electronic &&
            p.Specification.Key.ToString() == "C Minor"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }
}

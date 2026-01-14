using System.Net.Http.Json;
using FluentAssertions;
using MusicForge.Api.DTOs;
using Xunit;

namespace MusicForge.Integration.Tests;

public class EndToEndTests
{
    private readonly HttpClient _client;

    // API running on localhost:5001 as per docker-compose
    private const string BaseUrl = "http://localhost:5001";

    public EndToEndTests()
    {
        _client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
    }

    [Fact]
    public async Task CreateProject_ShouldReturnProjectId()
    {
        // Arrange - Use DTO, not Command
        var request = new CreateProjectRequest(
            Name: "E2E Test Project",
            Description: "Integration test generated",
            Genre: "Electronic",
            Mood: "Energetic",
            TempoBpm: 128,
            Key: "C Minor",
            DurationSeconds: 60,
            HasVocals: false,
            Lyrics: null
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/projects", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CreateProjectResponse>();
        result.Should().NotBeNull();
        result!.ProjectId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task HealthCheck_ShouldBeHealthy()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        content.Should().ContainEquivalentOf("Healthy");
    }
}

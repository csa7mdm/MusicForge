using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Moq; // Added
using MusicForge.Application.Interfaces;
using MusicForge.Infrastructure.Grpc;
using Xunit;
using InfraGrpc = MusicForge.Infrastructure.Grpc; // Alias

namespace MusicForge.Integration.Tests;

public class WorkerClientTests
{
    [Fact]
    public async Task HealthCheck_ShouldReturnStatus()
    {
        // Setup mock server or use a real address if configured
        // For unit/integration test without spinning up the actual python process (which is hard here), 
        // we might mainly verify the DI registration or use a TestServer for gRPC if possible.
        // However, gRPC.Net.Client can point to a localized test service.

        // Strategy: We can't easily spin up the Python server locally due to environment issues (ffmpeg).
        // So this test will be skipped or we assume a mock server.
        // But the goal is to test the C# Client wrapper logic.

        // We can mock the generated MusicWorker.MusicWorkerClient?
        // It's a key part of the wrapper.
        // The wrapper GrpcMusicWorkerClient takes the client in constructor.

        // Mocking the gRPC client
        var mockGrpcClient = new Moq.Mock<MusicWorker.MusicWorkerClient>();

        var response = new InfraGrpc.HealthResponse
        {
            Status = "healthy",
            GpuAvailable = true,
            GpuMemoryBytes = 8589934592
        };

        mockGrpcClient.Setup(c => c.HealthCheckAsync(It.IsAny<InfraGrpc.Empty>(), It.IsAny<Metadata>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .Returns(new AsyncUnaryCall<InfraGrpc.HealthResponse>(
                Task.FromResult(response),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { }));

        var client = new GrpcMusicWorkerClient(mockGrpcClient.Object);

        // Act
        var result = await client.HealthCheckAsync();

        // Assert
        result.Status.Should().Be("healthy");
        result.GpuAvailable.Should().BeTrue();
    }
}

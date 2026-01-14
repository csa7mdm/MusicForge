using Microsoft.AspNetCore.Http.HttpResults;
using MusicForge.Api.DTOs;
using MusicForge.Application.Interfaces;

namespace MusicForge.Api.Endpoints;

/// <summary>
/// Health and status endpoints.
/// </summary>
public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api")
        .WithTags("Health");

        group.MapGet("/health", GetHealth)
        .WithName("HealthCheck")
        .WithSummary("Check API health status");

        group.MapGet("/health/worker", GetWorkerHealth)
        .WithName("WorkerHealthCheck")
        .WithSummary("Check Python worker health");
    }

    private static Ok<HealthResponse> GetHealth()
    {
        return TypedResults.Ok(new HealthResponse(
        Status: "Healthy",
        Version: "1.0.0",
        WorkerConnected: false, // Will be updated when worker client is injected
        GpuAvailable: false
        ));
    }
    private static async Task<Results<Ok<HealthResponse>, StatusCodeHttpResult>> GetWorkerHealth(
    IMusicWorkerClient? workerClient,
    CancellationToken ct)
    {
        if (workerClient is null)
        {
            return TypedResults.Ok(new HealthResponse(
            Status: "Degraded",
            Version: "1.0.0",
            WorkerConnected: false,
            GpuAvailable: false
            ));
        }

        try
        {
            var health = await workerClient.HealthCheckAsync(ct);
            return TypedResults.Ok(new HealthResponse(
            Status: health.Status,
            Version: "1.0.0",
            WorkerConnected: true,
            GpuAvailable: health.GpuAvailable
            ));
        }
        catch
        {
            return TypedResults.StatusCode(503);
        }
    }
}

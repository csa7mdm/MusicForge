using Microsoft.AspNetCore.SignalR;
using MusicForge.Api.DTOs;

namespace MusicForge.Api.Hubs;

/// <summary>
/// SignalR hub for real-time generation progress updates.
/// </summary>
public sealed class GenerationHub : Hub
{
    /// <summary>
    /// Join a project's progress channel.
    /// </summary>
    public async Task JoinProject(Guid projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, projectId.ToString());
        await Clients.Caller.SendAsync("Joined", projectId);
    }

    /// <summary>
    /// Leave a project's progress channel.
    /// </summary>
    public async Task LeaveProject(Guid projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, projectId.ToString());
    }
}

/// <summary>
/// Extension methods for sending hub messages.
/// </summary>
public static class GenerationHubExtensions
{
    /// <summary>
    /// Send progress update to all clients watching a project.
    /// </summary>
    public static async Task SendProgressAsync(
    this IHubContext<GenerationHub> hub,
    Guid projectId,
    ProgressUpdate update)
    {
        await hub.Clients.Group(projectId.ToString())
        .SendAsync("ProgressUpdate", update);
    }

    /// <summary>
    /// Send completion notification.
    /// </summary>
    public static async Task SendCompletedAsync(
    this IHubContext<GenerationHub> hub,
    Guid projectId,
    GenerationResponse result)
    {
        await hub.Clients.Group(projectId.ToString())
        .SendAsync("GenerationCompleted", result);
    }

    /// <summary>
    /// Send error notification.
    /// </summary>
    public static async Task SendErrorAsync(
    this IHubContext<GenerationHub> hub,
    Guid projectId,
    string error)
    {
        await hub.Clients.Group(projectId.ToString())
        .SendAsync("GenerationFailed", new { Error = error });
    }
}

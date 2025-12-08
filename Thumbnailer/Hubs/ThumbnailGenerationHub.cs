using Microsoft.AspNetCore.SignalR;

namespace Thumbnailer.Hubs;

public sealed class ThumbnailGenerationHub(ILogger<ThumbnailGenerationHub> _logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client connected: {Context.ConnectionId}");
        await Clients.Caller.SendAsync("ReceiveConnectionId", $"{Context.ConnectionId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
        await Clients.All.SendAsync("OnDisconnectedAsync", $"Client disconnected: {Context.ConnectionId}");
    }

    public async Task SendMessage(string connId, string message)
    {
        _logger.LogInformation($"Received message from {Context.ConnectionId}: {message}");
        await Clients.Client(connId).SendAsync("SendMessage", message);
    }


}

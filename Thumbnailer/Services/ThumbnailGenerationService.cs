using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using Thumbnailer.Hubs;
using Thumbnailer.Models;

namespace Thumbnailer.Services;

public class ThumbnailGenerationService(
    ILogger<ThumbnailGenerationService> _logger,
    Channel<ThumbnailGenerationJob> _channel,
    ImageService _imageService,
    ConcurrentDictionary<string, ThumbnailGenerationStatus> _statusDictionary,
    IHubContext<ThumbnailGenerationHub> _hubContext
    ) : BackgroundService

{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Thumbnail Generation Service started.");
        await foreach (var job in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation("Processing thumbnail generation job {JobId}", job.Id);
                _statusDictionary[job.Id] = ThumbnailGenerationStatus.Processing;
                await _hubContext.Clients.Client(job.ConnId).SendAsync("JobProcessing", new { message = "Thumbnail job processing.", status = ThumbnailGenerationStatus.Processing.ToString() });
                await _imageService.GenerateThumbnailAsync(job.Id, job.OrginalFilePath, job.FolderPath);

                _logger.LogInformation("Thumbnail generation job {JobId} completed.", job.Id);
                _statusDictionary[job.Id] = ThumbnailGenerationStatus.Completed;
                await _hubContext.Clients.Client(job.ConnId).SendAsync("JobCompleted",
                     new { message = "Thumbnail job completed.", status = ThumbnailGenerationStatus.Completed.ToString() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing thumbnail generation job {JobId}", job.Id);
                _statusDictionary[job.Id] = ThumbnailGenerationStatus.Failed;
                await _hubContext.Clients.Client(job.ConnId).SendAsync("JobFailed",
                     new { message = "TThumbnail job failed.", status = ThumbnailGenerationStatus.Failed.ToString() });
            }
        }
    }
}

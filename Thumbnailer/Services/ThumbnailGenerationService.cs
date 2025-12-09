using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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
    IHubContext<ThumbnailGenerationHub> _hubContext,
    IChannel _rabbitMQChannel
    ) : BackgroundService

{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Thumbnail Generation Service started.");

        // resolve job using built-in channels
        //await foreach (var job in _channel.Reader.ReadAllAsync(stoppingToken))
        //{
        //    try
        //    {
        //        _logger.LogInformation("Processing thumbnail generation job {JobId}", job.Id);
        //        _statusDictionary[job.Id] = ThumbnailGenerationStatus.Processing;
        //        await _hubContext.Clients.Client(job.ConnId).SendAsync("JobProcessing", new { message = "Thumbnail job processing.", status = ThumbnailGenerationStatus.Processing.ToString() });
        //        await _imageService.GenerateThumbnailAsync(job.Id, job.OrginalFilePath, job.FolderPath);

        //        _logger.LogInformation("Thumbnail generation job {JobId} completed.", job.Id);
        //        _statusDictionary[job.Id] = ThumbnailGenerationStatus.Completed;
        //        await _hubContext.Clients.Client(job.ConnId).SendAsync("JobCompleted",
        //             new { message = "Thumbnail job completed.", status = ThumbnailGenerationStatus.Completed.ToString() });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error processing thumbnail generation job {JobId}", job.Id);
        //        _statusDictionary[job.Id] = ThumbnailGenerationStatus.Failed;
        //        await _hubContext.Clients.Client(job.ConnId).SendAsync("JobFailed",
        //             new { message = "TThumbnail job failed.", status = ThumbnailGenerationStatus.Failed.ToString() });
        //    }
        //}

        // resolve job using rabbitMQ channel
        var consumer = new AsyncEventingBasicConsumer(_rabbitMQChannel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = System.Text.Encoding.UTF8.GetString(body);
            var job = System.Text.Json.JsonSerializer.Deserialize<ThumbnailGenerationJob>(message);
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
            await _rabbitMQChannel.BasicAckAsync(ea.DeliveryTag, false);
        };

        await _rabbitMQChannel.BasicConsumeAsync(
            queue: "thumbnail_jobs",
            autoAck: false,
            consumer: consumer);
    }
}

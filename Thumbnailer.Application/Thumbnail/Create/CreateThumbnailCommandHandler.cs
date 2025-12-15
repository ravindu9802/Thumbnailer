using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Threading.Channels;
using Thumbnailer.Application.Hubs;
using Thumbnailer.Domain.Contracts;
using Thumbnailer.Domain.Models;
using Thumbnailer.Domain.Services;

namespace Thumbnailer.Application.Thumbnail.Create;

internal sealed class CreateThumbnailCommandHandler(
    Channel<ThumbnailGenerationJob> _channel,
    ConcurrentDictionary<string, ThumbnailGenerationStatus> _statusDictionary,
    IConfiguration _configuration,
    IImageService _imageService,
    IHubContext<ThumbnailGenerationHub> _hubContext
    //IChannel _rabbitMQChannel
    ) : IRequestHandler<CreateThumbnailCommand, CreateThumbnailCommandResponse>
{
    public async Task<CreateThumbnailCommandResponse> Handle(CreateThumbnailCommand request, CancellationToken cancellationToken)
    {
        IFormFile file = request.File;
        string connId = request.ConnId;
        if (file == null || file.Length == 0)
            throw new Exception("No file uploaded");

        string basePath = _configuration.GetSection("BaseUploadPath").Value!;
        string folderId = Guid.NewGuid().ToString();
        string folderPath = Path.Combine(basePath, folderId);
        string fileName = "original" + Path.GetExtension(file.FileName);
        string imagePath = await _imageService.UploadImageAsync(file, folderPath, fileName);

        var job = new ThumbnailGenerationJob(folderId, imagePath, folderPath, request.ConnId);
        await _channel.Writer.WriteAsync(job);

        _statusDictionary[folderId] = ThumbnailGenerationStatus.Queued;
        //await _rabbitMQChannel.BasicPublishAsync(
        //    exchange: "",
        //    routingKey: "thumbnail_jobs",
        //    mandatory: true,
        //    basicProperties: new BasicProperties { Persistent = true},
        //    body: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(job))
        //);
        await _hubContext.Clients.Client(connId).SendAsync("JobAccepted",
            new { message = "Thumbnail job accpeted.", status = ThumbnailGenerationStatus.Queued.ToString() });

        string url = $"{request.BaseUrl}thumbnail/status/{folderId}";
        return new CreateThumbnailCommandResponse(true, url, new { id = folderId, status = _statusDictionary[folderId].ToString() });
    }
}

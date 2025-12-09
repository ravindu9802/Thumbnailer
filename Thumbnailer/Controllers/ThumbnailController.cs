using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Thumbnailer.Hubs;
using Thumbnailer.Models;
using Thumbnailer.Services;

namespace Thumbnailer.Controllers;

[ApiController]
[Route("[controller]")]
public class ThumbnailController(
    Channel<ThumbnailGenerationJob> _channel,
    ImageService _imageService,
    ConcurrentDictionary<string, ThumbnailGenerationStatus> _statusDictionary,
    IHubContext<ThumbnailGenerationHub> _hubContext,
    IChannel _rabbitMQChannel
    ) : ControllerBase
{

    private readonly string baseFolderPath = "uploads";
    private readonly int[] resizeWidths = [5, 25, 50, 75, 100];

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateThumbnail(IFormFile file, [FromForm] string connId)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        string folderId = Guid.NewGuid().ToString();
        string folderPath = Path.Combine(baseFolderPath, folderId);
        string fileName = "original" + Path.GetExtension(file.FileName);
        string imagePath = await _imageService.UploadImageAsync(file, folderPath, fileName);

        var job = new ThumbnailGenerationJob(folderId, imagePath, folderPath, connId);
        //await _channel.Writer.WriteAsync(job);

        // Set status or send notification to signalR
        _statusDictionary[folderId] = ThumbnailGenerationStatus.Queued;
        await _rabbitMQChannel.BasicPublishAsync(
            exchange: "",
            routingKey: "thumbnail_jobs",
            mandatory: true,
            basicProperties: new BasicProperties { Persistent = true},
            body: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(job))
        );
        await _hubContext.Clients.Client(connId).SendAsync("JobAccepted", 
            new { message = "Thumbnail job accpeted.", status = ThumbnailGenerationStatus.Queued.ToString() });

        string url = $"{Request.Scheme}://{Request.Host}/thumbnail/status/{folderId}";
        return Accepted(url, new { id = folderId, status = _statusDictionary[folderId].ToString() });
    }

    [HttpGet("status/{id}")]
    public IActionResult GetStatus([FromRoute] string id)
    {
        if (!_statusDictionary.TryGetValue(id, out var status))
        {
            return NotFound("ID not found");
        }

        var result = new { id, status = status.ToString(), links = new Dictionary<string, string>() };

        if (status == ThumbnailGenerationStatus.Completed)
        {
            string baseThumbnailUrl = $"{Request.Scheme}://{Request.Host}/thumbnail/";
            var links = resizeWidths.ToDictionary(
                width => $"w_{width}",
                width => $"{baseThumbnailUrl}{id}?width={width}"
                );
            links.Add("original", $"{baseThumbnailUrl}{id}");
            result = result with { links = links };
        }

        return Ok(result);
    }

    [HttpGet("{id}")]
    public IActionResult GetThumbnail([FromRoute] string id, [FromQuery] int? width)
    {
        string folderPath = Path.Combine(baseFolderPath, id);
        ImageResult result = _imageService.GetThumbnailAsync(folderPath, width);
        return File(result.Stream, result.ContentType);
    }
}

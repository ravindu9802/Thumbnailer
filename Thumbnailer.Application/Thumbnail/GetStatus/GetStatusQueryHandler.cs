using System.Collections.Concurrent;
using Thumbnailer.Application.Abstractions;
using Thumbnailer.Domain.Models;
using Thumbnailer.Domain.Services;

namespace Thumbnailer.Application.Thumbnail.GetStatus;

internal sealed class GetStatusQueryHandler(
    ConcurrentDictionary<string, ThumbnailGenerationStatus> _statusDictionary,
     IImageService _imageService
    ) : IQueryHandler<GetStatusQuery, GetStatusQueryResponse>
{
    public async Task<GetStatusQueryResponse> Handle(GetStatusQuery request, CancellationToken cancellationToken)
    {
        if (!_statusDictionary.TryGetValue(request.Id, out var status))
        {
            throw new Exception($"ID not found: {request.Id}");
        }

        var result = new GetStatusQueryResponse(request.Id, status.ToString(), new Dictionary<string, string>());

        if (status == ThumbnailGenerationStatus.Completed)
        {
            string baseThumbnailUrl = $"{request.BaseUrl}thumbnail/";
            var links = _imageService.GetResizeWidths().ToDictionary(
                width => $"w_{width}",
                width => $"{baseThumbnailUrl}{request.Id}?width={width}"
                );
            links.Add("original", $"{baseThumbnailUrl}{request.Id}");
            result = result with { Links = links };
        }

        return result;
    }
}

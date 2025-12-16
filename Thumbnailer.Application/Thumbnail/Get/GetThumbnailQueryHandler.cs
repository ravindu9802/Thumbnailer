using Microsoft.Extensions.Configuration;
using Thumbnailer.Application.Abstractions;
using Thumbnailer.Domain.Models;
using Thumbnailer.Domain.Services;

namespace Thumbnailer.Application.Thumbnail.Get;

internal sealed class GetThumbnailQueryHandler(
    IImageService _imageService,
    IConfiguration _configuration
    ) : IQueryHandler<GetThumbnailQuery, GetThumbnailQueryResponse>
{
    public async Task<GetThumbnailQueryResponse> Handle(GetThumbnailQuery request, CancellationToken cancellationToken)
    {
        string folderPath = Path.Combine(_configuration.GetSection("BaseUploadPath").Value!, request.Id);
        ImageResult result = _imageService.GetThumbnail(folderPath, request.Width);
        return new GetThumbnailQueryResponse(result.Stream, result.ContentType);
    }
}

using Microsoft.AspNetCore.Http;
using Thumbnailer.Domain.Models;

namespace Thumbnailer.Domain.Services;

public interface IImageService
{
    int[] GetResizeWidths();
    Task<string> UploadImageAsync(IFormFile file, string folderPath, string fileName);
    Task GenerateThumbnailAsync(string id, string orginalFilePath, string folderPath);
    ImageResult GetThumbnail(string folderPath, int? width);
}

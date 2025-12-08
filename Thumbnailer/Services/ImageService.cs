using Microsoft.AspNetCore.StaticFiles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Thumbnailer.Models;

namespace Thumbnailer.Services;

public class ImageService()
{
    private readonly int[] resizeWidths = [5, 25, 50, 75, 100];

    public async Task<string> UploadImageAsync(IFormFile file, string folderPath, string fileName)
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            throw new ArgumentException("Folder path cannot be null or empty", nameof(folderPath));
        }

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var imagePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(imagePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return imagePath;
    }

    public async Task GenerateThumbnailAsync(string id, string orginalFilePath, string folderPath)
    {
        foreach (var width in resizeWidths)
        {
            string outputPath = Path.Combine(folderPath, $"w_{width}{Path.GetExtension(orginalFilePath)}");
            await ResizeImageAsync(GetFileStream(orginalFilePath), outputPath, width, 0);
            Task.Delay(500).Wait();
        }
    }

    public ImageResult GetThumbnailAsync(string folderPath, int? width)
    {
        string? filePath;
        if (width is null)
        {
            filePath = FindFileIgnoringExtension(folderPath, "original");
        }
        else
        {
            filePath = FindFileIgnoringExtension(folderPath, $"w_{width}");
        }

        if (filePath is null)
            throw new FileNotFoundException("Thumbnail not found");

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(filePath, out string contentType))
            contentType = "application/octet-stream";

        return new ImageResult(new FileStream(filePath, FileMode.Open, FileAccess.Read), contentType);
    }

    private string? FindFileIgnoringExtension(string folderPath, string fileNameWithoutExtension)
    {
        var file = Directory
            .EnumerateFiles(folderPath)
            .FirstOrDefault(f =>
                Path.GetFileNameWithoutExtension(f)
                    .Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));

        return file;
    }

    private Stream GetFileStream(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("File not found", path);

        return new FileStream(path, FileMode.Open, FileAccess.Read);
    }

    private async Task ResizeImageAsync(Stream input, string outputPath, int width, int height)
    {
        using (var image = await Image.LoadAsync(input))
        {
            image.Mutate(x => x.Resize(width, height));
            await image.SaveAsync(outputPath);
        }
    }
}
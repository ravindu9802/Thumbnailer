namespace Thumbnailer.Models;

public record ThumbnailGenerationJob(string Id, string OrginalFilePath, string FolderPath, string ConnId);
namespace Thumbnailer.Domain.Contracts;

public record ThumbnailGenerationJob(string Id, string OrginalFilePath, string FolderPath, string ConnId);
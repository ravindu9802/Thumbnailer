namespace Thumbnailer.Application.Thumbnail.Create;

public sealed record CreateThumbnailCommandResponse(bool Success, string? Url, object? Result);
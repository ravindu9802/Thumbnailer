namespace Thumbnailer.Application.Thumbnail.GetStatus;

public sealed record GetStatusQueryResponse(string Id, string Status, Dictionary<string, string>? Links);

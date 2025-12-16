using Thumbnailer.Application.Abstractions;

namespace Thumbnailer.Application.Thumbnail.Get;

public sealed record GetThumbnailQuery(string Id, int? Width) 
    : IQuery<GetThumbnailQueryResponse>;

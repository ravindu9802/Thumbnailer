using MediatR;

namespace Thumbnailer.Application.Thumbnail.Get;

public sealed record GetThumbnailQuery(string Id, int? Width) : IRequest<GetThumbnailQueryResponse>;

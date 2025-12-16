using Thumbnailer.Application.Abstractions;

namespace Thumbnailer.Application.Thumbnail.GetStatus;

public record class GetStatusQuery(string Id, string BaseUrl)
    : IQuery<GetStatusQueryResponse>;
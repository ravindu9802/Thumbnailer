using MediatR;

namespace Thumbnailer.Application.Thumbnail.GetStatus;

public record class GetStatusQuery(string Id, string BaseUrl) : IRequest<GetStatusQueryResponse>;
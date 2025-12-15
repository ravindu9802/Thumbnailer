using MediatR;
using Microsoft.AspNetCore.Http;

namespace Thumbnailer.Application.Thumbnail.Create;

public sealed record CreateThumbnailCommand(IFormFile File, string ConnId, string BaseUrl) : IRequest<CreateThumbnailCommandResponse>;

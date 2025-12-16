using Microsoft.AspNetCore.Http;
using Thumbnailer.Application.Abstractions;

namespace Thumbnailer.Application.Thumbnail.Create;

public sealed record CreateThumbnailCommand(IFormFile File, string ConnId, string BaseUrl) 
    : ICommand<CreateThumbnailCommandResponse>;

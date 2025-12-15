using MediatR;
using Microsoft.AspNetCore.Mvc;
using Thumbnailer.Application.Thumbnail.Create;
using Thumbnailer.Application.Thumbnail.Get;
using Thumbnailer.Application.Thumbnail.GetStatus;

namespace Thumbnailer.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ThumbnailController(
    IMediator _mediator
    ) : ControllerBase
{
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateThumbnail(IFormFile file, [FromForm] string connId)
    {
        CreateThumbnailCommandResponse result = await _mediator.Send(new CreateThumbnailCommand(file, connId, GetBaseUrl()));
        return Accepted(result.Url, result.Result);
    }

    [HttpGet("status/{id}")]
    public async Task<IActionResult> GetStatus([FromRoute] string id)
    {
        return Ok(await _mediator.Send(new GetStatusQuery(id, GetBaseUrl())));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetThumbnail([FromRoute] string id, [FromQuery] int? width)
    {
        GetThumbnailQueryResponse result = await _mediator.Send(new GetThumbnailQuery(id, width));
        return File(result.Stream, result.ContentType);
    }

    #region private methods
    private string GetBaseUrl()
    {
        var scheme = Request.Headers["X-Forwarded-Proto"].FirstOrDefault()
                     ?? Request.Scheme;

        var host = Request.Headers["X-Forwarded-Host"].FirstOrDefault()
                   ?? Request.Host.Value;

        return $"{scheme}://{host}/";
    }
    #endregion
}

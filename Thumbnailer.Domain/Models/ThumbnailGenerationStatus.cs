namespace Thumbnailer.Domain.Models;

public enum ThumbnailGenerationStatus
{
    Queued,
    Processing,
    Completed,
    Failed
}

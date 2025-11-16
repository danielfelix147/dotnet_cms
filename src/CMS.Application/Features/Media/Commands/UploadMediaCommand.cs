using MediatR;

namespace CMS.Application.Features.Media.Commands;

public class UploadMediaCommand : IRequest<Guid>
{
    public Guid SiteId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string? Description { get; set; }
}

namespace CMS.Application.DTOs;

public class MediaDto
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileType { get; set; } = string.Empty; // Image, Document, Video, etc.
    public int? Width { get; set; }
    public int? Height { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
}

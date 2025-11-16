namespace CMS.Application.DTOs;

public class FileDto
{
    public Guid Id { get; set; }
    public string FileId { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Title { get; set; }
}

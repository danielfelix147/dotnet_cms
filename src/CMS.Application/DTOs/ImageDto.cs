namespace CMS.Application.DTOs;

public class ImageDto
{
    public Guid Id { get; set; }
    public string ImageId { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public string? Title { get; set; }
}

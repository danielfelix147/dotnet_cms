namespace CMS.Application.DTOs;

public class DestinationDto
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public string DestinationId { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublished { get; set; }
    public List<TourDto> Tours { get; set; } = new();
}

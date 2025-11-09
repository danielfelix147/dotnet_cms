namespace CMS.Application.DTOs;

public class TourDto
{
    public Guid Id { get; set; }
    public Guid DestinationId { get; set; }
    public string TourId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsPublished { get; set; }
}

namespace CMS.Domain.Entities;

public class Tour : BaseEntity
{
    public Guid DestinationId { get; set; }
    public string TourId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsPublished { get; set; }
    
    // Navigation properties
    public Destination Destination { get; set; } = null!;
}

namespace CMS.Domain.Entities;

public class Destination : BaseEntity
{
    public Guid SiteId { get; set; }
    public string DestinationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublished { get; set; }
    
    // Navigation properties
    public Site Site { get; set; } = null!;
    public ICollection<Tour> Tours { get; set; } = new List<Tour>();
}

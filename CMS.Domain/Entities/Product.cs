namespace CMS.Domain.Entities;

public class Product : BaseEntity
{
    public Guid SiteId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsPublished { get; set; }
    
    // Navigation properties
    public Site Site { get; set; } = null!;
}

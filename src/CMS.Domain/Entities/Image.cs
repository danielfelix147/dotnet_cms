namespace CMS.Domain.Entities;

public class Image : BaseEntity
{
    public string ImageId { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public string? Title { get; set; }
    public long FileSize { get; set; }
    public string? MimeType { get; set; }
    
    // Generic foreign key for polymorphic association
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty; // Page, Product, Destination, Tour
}

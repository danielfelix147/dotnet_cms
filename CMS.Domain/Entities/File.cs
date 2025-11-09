namespace CMS.Domain.Entities;

public class File : BaseEntity
{
    public string FileId { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Title { get; set; }
    public long FileSize { get; set; }
    public string? MimeType { get; set; }
    
    // Generic foreign key for polymorphic association
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty; // Page, Product, Destination, Tour
}

namespace CMS.Domain.Entities;

public class PageContent : BaseEntity
{
    public Guid PageId { get; set; }
    public string ContentId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // HTML content
    public int Order { get; set; }
    
    // Navigation properties
    public Page Page { get; set; } = null!;
}

namespace CMS.Domain.Entities;

public class Page : BaseEntity
{
    public Guid SiteId { get; set; }
    public string PageId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublished { get; set; }
    
    // Navigation properties
    public Site Site { get; set; } = null!;
    public ICollection<PageContent> Contents { get; set; } = new List<PageContent>();
}

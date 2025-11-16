namespace CMS.Domain.Entities;

public class Plugin : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string SystemName { get; set; } = string.Empty; // PageManagement, ProductManagement, TravelManagement
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public ICollection<SitePlugin> SitePlugins { get; set; } = new List<SitePlugin>();
}

namespace CMS.Domain.Entities;

public class Site : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public ICollection<SiteUser> SiteUsers { get; set; } = new List<SiteUser>();
    public ICollection<SitePlugin> SitePlugins { get; set; } = new List<SitePlugin>();
}

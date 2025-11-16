namespace CMS.Domain.Entities;

public class SitePlugin : BaseEntity
{
    public Guid SiteId { get; set; }
    public Guid PluginId { get; set; }
    public bool IsEnabled { get; set; }
    public string? Configuration { get; set; } // JSON configuration for plugin
    
    // Navigation properties
    public Site Site { get; set; } = null!;
    public Plugin Plugin { get; set; } = null!;
}

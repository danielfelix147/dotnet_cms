namespace CMS.Domain.Entities;

public class SiteUser : BaseEntity
{
    public Guid SiteId { get; set; }
    public string UserId { get; set; } = string.Empty; // IdentityUser.Id
    public string Role { get; set; } = string.Empty; // SiteAdmin, SiteEditor, SiteViewer
    
    // Navigation properties
    public Site Site { get; set; } = null!;
}

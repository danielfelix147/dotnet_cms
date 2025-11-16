namespace CMS.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // Admin, Editor, Viewer
    public bool IsActive { get; set; }
    
    // Navigation properties
    public ICollection<SiteUser> SiteUsers { get; set; } = new List<SiteUser>();
}

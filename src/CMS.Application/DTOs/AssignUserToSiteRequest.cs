namespace CMS.Application.DTOs;

public class AssignUserToSiteRequest
{
    public string UserId { get; set; } = string.Empty;
    public Guid SiteId { get; set; }
    public string Role { get; set; } = "Editor";
}

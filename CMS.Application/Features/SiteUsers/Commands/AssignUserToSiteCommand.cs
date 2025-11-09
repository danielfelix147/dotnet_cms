using MediatR;

namespace CMS.Application.Features.SiteUsers.Commands;

public class AssignUserToSiteCommand : IRequest<bool>
{
    public string UserId { get; set; } = string.Empty;
    public Guid SiteId { get; set; }
    public string Role { get; set; } = "Editor"; // Default role
}

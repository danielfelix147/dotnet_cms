using MediatR;

namespace CMS.Application.Features.SiteUsers.Commands;

public class RemoveUserFromSiteCommand : IRequest<bool>
{
    public string UserId { get; set; } = string.Empty;
    public Guid SiteId { get; set; }
}

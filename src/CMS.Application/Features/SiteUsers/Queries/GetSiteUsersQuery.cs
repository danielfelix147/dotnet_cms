using MediatR;
using CMS.Application.DTOs;

namespace CMS.Application.Features.SiteUsers.Queries;

public class GetSiteUsersQuery : IRequest<IEnumerable<UserDto>>
{
    public Guid SiteId { get; set; }
}

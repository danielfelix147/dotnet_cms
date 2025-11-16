using MediatR;
using CMS.Application.DTOs;

namespace CMS.Application.Features.Sites.Queries;

public class GetSiteByIdQuery : IRequest<SiteDto?>
{
    public Guid SiteId { get; set; }
}

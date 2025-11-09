using MediatR;
using CMS.Application.DTOs;

namespace CMS.Application.Features.Pages.Queries;

public class GetPagesBySiteIdQuery : IRequest<IEnumerable<PageDto>>
{
    public Guid SiteId { get; set; }
}

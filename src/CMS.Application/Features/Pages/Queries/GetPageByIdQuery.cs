using MediatR;
using CMS.Application.DTOs;

namespace CMS.Application.Features.Pages.Queries;

public class GetPageByIdQuery : IRequest<PageDto?>
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
}

using MediatR;
using CMS.Application.DTOs;

namespace CMS.Application.Features.Destinations.Queries;

public class GetDestinationsBySiteIdQuery : IRequest<IEnumerable<DestinationDto>>
{
    public Guid SiteId { get; set; }
}

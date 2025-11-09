using MediatR;
using CMS.Application.DTOs;

namespace CMS.Application.Features.Destinations.Queries;

public class GetDestinationByIdQuery : IRequest<DestinationDto?>
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
}

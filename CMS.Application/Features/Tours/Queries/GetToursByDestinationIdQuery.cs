using MediatR;
using CMS.Application.DTOs;

namespace CMS.Application.Features.Tours.Queries;

public class GetToursByDestinationIdQuery : IRequest<IEnumerable<TourDto>>
{
    public Guid DestinationId { get; set; }
}

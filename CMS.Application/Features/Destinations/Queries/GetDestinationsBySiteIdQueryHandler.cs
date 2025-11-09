using MediatR;
using CMS.Application.Mappings;
using CMS.Application.DTOs;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.Destinations.Queries;

public class GetDestinationsBySiteIdQueryHandler : IRequestHandler<GetDestinationsBySiteIdQuery, IEnumerable<DestinationDto>>
{
    private readonly IRepository<Destination> _destinationRepository;
    

    public GetDestinationsBySiteIdQueryHandler(IRepository<Destination> destinationRepository)
    {
        _destinationRepository = destinationRepository;
        
    }

    public async Task<IEnumerable<DestinationDto>> Handle(GetDestinationsBySiteIdQuery request, CancellationToken cancellationToken)
    {
        var destinations = await _destinationRepository.FindAsync(d => d.SiteId == request.SiteId && !d.IsDeleted);
        return destinations.ToDto();
    }
}

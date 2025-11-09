using MediatR;
using CMS.Application.Mappings;
using CMS.Application.DTOs;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.Destinations.Queries;

public class GetDestinationByIdQueryHandler : IRequestHandler<GetDestinationByIdQuery, DestinationDto?>
{
    private readonly IRepository<Destination> _destinationRepository;
    

    public GetDestinationByIdQueryHandler(IRepository<Destination> destinationRepository)
    {
        _destinationRepository = destinationRepository;
        
    }

    public async Task<DestinationDto?> Handle(GetDestinationByIdQuery request, CancellationToken cancellationToken)
    {
        var destination = await _destinationRepository.GetByIdAsync(request.Id);
        
        if (destination == null || destination.SiteId != request.SiteId || destination.IsDeleted)
            return null;

        return destination.ToDto();
    }
}

using MediatR;
using CMS.Application.Mappings;
using CMS.Application.DTOs;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.Tours.Queries;

public class GetToursByDestinationIdQueryHandler : IRequestHandler<GetToursByDestinationIdQuery, IEnumerable<TourDto>>
{
    private readonly IRepository<Tour> _tourRepository;
    

    public GetToursByDestinationIdQueryHandler(IRepository<Tour> tourRepository)
    {
        _tourRepository = tourRepository;
        
    }

    public async Task<IEnumerable<TourDto>> Handle(GetToursByDestinationIdQuery request, CancellationToken cancellationToken)
    {
        var tours = await _tourRepository.FindAsync(t => t.DestinationId == request.DestinationId && !t.IsDeleted);
        return tours.ToDto();
    }
}

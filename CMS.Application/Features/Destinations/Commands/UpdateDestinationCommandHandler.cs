using MediatR;
using CMS.Application.Mappings;
using CMS.Application.DTOs;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.Destinations.Commands;

public class UpdateDestinationCommandHandler : IRequestHandler<UpdateDestinationCommand, DestinationDto?>
{
    private readonly IRepository<Destination> _destinationRepository;
    private readonly IUnitOfWork _unitOfWork;
    

    public UpdateDestinationCommandHandler(IRepository<Destination> destinationRepository, IUnitOfWork unitOfWork)
    {
        _destinationRepository = destinationRepository;
        _unitOfWork = unitOfWork;
        
    }

    public async Task<DestinationDto?> Handle(UpdateDestinationCommand request, CancellationToken cancellationToken)
    {
        var destination = await _destinationRepository.GetByIdAsync(request.Id);
        
        if (destination == null || destination.SiteId != request.SiteId)
            return null;

        destination.DestinationId = request.DestinationId;
        destination.Name = request.Name;
        destination.Description = request.Description;
        destination.IsPublished = request.IsPublished;
        destination.UpdatedAt = DateTime.UtcNow;

        await _destinationRepository.UpdateAsync(destination);
        await _unitOfWork.SaveChangesAsync();

        return destination.ToDto();
    }
}

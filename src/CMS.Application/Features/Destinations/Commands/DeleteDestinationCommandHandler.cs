using MediatR;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.Destinations.Commands;

public class DeleteDestinationCommandHandler : IRequestHandler<DeleteDestinationCommand, bool>
{
    private readonly IRepository<Destination> _destinationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDestinationCommandHandler(IRepository<Destination> destinationRepository, IUnitOfWork unitOfWork)
    {
        _destinationRepository = destinationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteDestinationCommand request, CancellationToken cancellationToken)
    {
        var destination = await _destinationRepository.GetByIdAsync(request.Id);
        
        if (destination == null || destination.SiteId != request.SiteId)
            return false;

        destination.IsDeleted = true;
        destination.UpdatedAt = DateTime.UtcNow;

        await _destinationRepository.UpdateAsync(destination);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

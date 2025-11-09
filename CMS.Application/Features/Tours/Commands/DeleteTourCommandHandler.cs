using MediatR;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.Tours.Commands;

public class DeleteTourCommandHandler : IRequestHandler<DeleteTourCommand, bool>
{
    private readonly IRepository<Tour> _tourRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTourCommandHandler(IRepository<Tour> tourRepository, IUnitOfWork unitOfWork)
    {
        _tourRepository = tourRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteTourCommand request, CancellationToken cancellationToken)
    {
        var tour = await _tourRepository.GetByIdAsync(request.Id);
        
        if (tour == null || tour.DestinationId != request.DestinationId)
            return false;

        tour.IsDeleted = true;
        tour.UpdatedAt = DateTime.UtcNow;

        await _tourRepository.UpdateAsync(tour);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

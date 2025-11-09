using MediatR;
using CMS.Application.Mappings;
using CMS.Application.DTOs;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.Tours.Commands;

public class UpdateTourCommandHandler : IRequestHandler<UpdateTourCommand, TourDto?>
{
    private readonly IRepository<Tour> _tourRepository;
    private readonly IUnitOfWork _unitOfWork;
    

    public UpdateTourCommandHandler(IRepository<Tour> tourRepository, IUnitOfWork unitOfWork)
    {
        _tourRepository = tourRepository;
        _unitOfWork = unitOfWork;
        
    }

    public async Task<TourDto?> Handle(UpdateTourCommand request, CancellationToken cancellationToken)
    {
        var tour = await _tourRepository.GetByIdAsync(request.Id);
        
        if (tour == null || tour.DestinationId != request.DestinationId)
            return null;

        tour.TourId = request.TourId;
        tour.Name = request.Name;
        tour.Description = request.Description;
        tour.Price = request.Price;
        tour.IsPublished = request.IsPublished;
        tour.UpdatedAt = DateTime.UtcNow;

        await _tourRepository.UpdateAsync(tour);
        await _unitOfWork.SaveChangesAsync();

        return tour.ToDto();
    }
}

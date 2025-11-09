using MediatR;
using CMS.Application.Mappings;
using CMS.Application.DTOs;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.Tours.Commands;

public class CreateTourCommandHandler : IRequestHandler<CreateTourCommand, TourDto>
{
    private readonly IRepository<Tour> _tourRepository;
    private readonly IUnitOfWork _unitOfWork;
    

    public CreateTourCommandHandler(IRepository<Tour> tourRepository, IUnitOfWork unitOfWork)
    {
        _tourRepository = tourRepository;
        _unitOfWork = unitOfWork;
        
    }

    public async Task<TourDto> Handle(CreateTourCommand request, CancellationToken cancellationToken)
    {
        var tour = new Tour
        {
            Id = Guid.NewGuid(),
            DestinationId = request.DestinationId,
            TourId = request.TourId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            IsPublished = request.IsPublished,
            CreatedAt = DateTime.UtcNow
        };

        await _tourRepository.AddAsync(tour);
        await _unitOfWork.SaveChangesAsync();

        return tour.ToDto();
    }
}

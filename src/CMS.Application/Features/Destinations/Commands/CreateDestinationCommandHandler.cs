using MediatR;
using CMS.Application.Mappings;
using CMS.Application.DTOs;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.Destinations.Commands;

public class CreateDestinationCommandHandler : IRequestHandler<CreateDestinationCommand, DestinationDto>
{
    private readonly IRepository<Destination> _destinationRepository;
    private readonly IUnitOfWork _unitOfWork;
    

    public CreateDestinationCommandHandler(IRepository<Destination> destinationRepository, IUnitOfWork unitOfWork)
    {
        _destinationRepository = destinationRepository;
        _unitOfWork = unitOfWork;
        
    }

    public async Task<DestinationDto> Handle(CreateDestinationCommand request, CancellationToken cancellationToken)
    {
        var destination = new Destination
        {
            Id = Guid.NewGuid(),
            SiteId = request.SiteId,
            DestinationId = request.DestinationId,
            Name = request.Name,
            Description = request.Description,
            IsPublished = request.IsPublished,
            CreatedAt = DateTime.UtcNow
        };

        await _destinationRepository.AddAsync(destination);
        await _unitOfWork.SaveChangesAsync();

        return destination.ToDto();
    }
}

using MediatR;
using CMS.Application.DTOs;
using CMS.Application.Mappings;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.Sites.Commands;

public class CreateSiteCommandHandler : IRequestHandler<CreateSiteCommand, SiteDto>
{
    private readonly ISiteRepository _siteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSiteCommandHandler(ISiteRepository siteRepository, IUnitOfWork unitOfWork)
    {
        _siteRepository = siteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SiteDto> Handle(CreateSiteCommand request, CancellationToken cancellationToken)
    {
        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Domain = request.Domain,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _siteRepository.AddAsync(site);
        await _unitOfWork.SaveChangesAsync();

        return site.ToDto();
    }
}

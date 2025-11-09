using MediatR;
using CMS.Application.DTOs;
using CMS.Application.Mappings;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.Sites.Commands;

public class UpdateSiteCommandHandler : IRequestHandler<UpdateSiteCommand, SiteDto?>
{
    private readonly ISiteRepository _siteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSiteCommandHandler(ISiteRepository siteRepository, IUnitOfWork unitOfWork)
    {
        _siteRepository = siteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SiteDto?> Handle(UpdateSiteCommand request, CancellationToken cancellationToken)
    {
        var site = await _siteRepository.GetByIdAsync(request.Id);
        
        if (site == null)
            return null;

        site.Name = request.Name;
        site.Domain = request.Domain;
        site.Description = request.Description;
        site.IsActive = request.IsActive;
        site.UpdatedAt = DateTime.UtcNow;

        await _siteRepository.UpdateAsync(site);
        await _unitOfWork.SaveChangesAsync();

        return site.ToDto();
    }
}

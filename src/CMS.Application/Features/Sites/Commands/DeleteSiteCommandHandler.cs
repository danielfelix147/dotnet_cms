using MediatR;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.Sites.Commands;

public class DeleteSiteCommandHandler : IRequestHandler<DeleteSiteCommand, bool>
{
    private readonly ISiteRepository _siteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSiteCommandHandler(ISiteRepository siteRepository, IUnitOfWork unitOfWork)
    {
        _siteRepository = siteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteSiteCommand request, CancellationToken cancellationToken)
    {
        var site = await _siteRepository.GetByIdAsync(request.Id);
        
        if (site == null)
            return false;

        // Soft delete
        site.IsDeleted = true;
        site.UpdatedAt = DateTime.UtcNow;

        await _siteRepository.UpdateAsync(site);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

using MediatR;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.SiteUsers.Commands;

public class AssignUserToSiteCommandHandler : IRequestHandler<AssignUserToSiteCommand, bool>
{
    private readonly IRepository<SiteUser> _siteUserRepository;
    private readonly IRepository<Site> _siteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignUserToSiteCommandHandler(
        IRepository<SiteUser> siteUserRepository,
        IRepository<Site> siteRepository,
        IUnitOfWork unitOfWork)
    {
        _siteUserRepository = siteUserRepository;
        _siteRepository = siteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(AssignUserToSiteCommand request, CancellationToken cancellationToken)
    {
        var site = await _siteRepository.GetByIdAsync(request.SiteId);
        if (site == null)
            return false;

        // Check if user is already assigned to this site
        var existing = await _siteUserRepository.FindAsync(su => 
            su.SiteId == request.SiteId && su.UserId == request.UserId);
        
        if (existing.Any())
        {
            // Update role if already assigned
            var siteUser = existing.First();
            siteUser.Role = request.Role;
            siteUser.UpdatedAt = DateTime.UtcNow;
            await _siteUserRepository.UpdateAsync(siteUser);
        }
        else
        {
            // Create new assignment
            var siteUser = new SiteUser
            {
                Id = Guid.NewGuid(),
                SiteId = request.SiteId,
                UserId = request.UserId,
                Role = request.Role,
                CreatedAt = DateTime.UtcNow
            };

            await _siteUserRepository.AddAsync(siteUser);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}

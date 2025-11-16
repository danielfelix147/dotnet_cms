using MediatR;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.SiteUsers.Commands;

public class RemoveUserFromSiteCommandHandler : IRequestHandler<RemoveUserFromSiteCommand, bool>
{
    private readonly IRepository<SiteUser> _siteUserRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveUserFromSiteCommandHandler(
        IRepository<SiteUser> siteUserRepository,
        IUnitOfWork unitOfWork)
    {
        _siteUserRepository = siteUserRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(RemoveUserFromSiteCommand request, CancellationToken cancellationToken)
    {
        var siteUsers = await _siteUserRepository.FindAsync(su => 
            su.SiteId == request.SiteId && su.UserId == request.UserId);
        
        var siteUser = siteUsers.FirstOrDefault();
        if (siteUser == null)
            return false;

        await _siteUserRepository.DeleteAsync(siteUser);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}

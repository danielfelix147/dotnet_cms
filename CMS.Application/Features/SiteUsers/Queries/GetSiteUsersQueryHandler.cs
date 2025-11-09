using MediatR;
using CMS.Application.DTOs;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CMS.Application.Features.SiteUsers.Queries;

public class GetSiteUsersQueryHandler : IRequestHandler<GetSiteUsersQuery, IEnumerable<UserDto>>
{
    private readonly IRepository<SiteUser> _siteUserRepository;
    private readonly UserManager<IdentityUser> _userManager;

    public GetSiteUsersQueryHandler(
        IRepository<SiteUser> siteUserRepository,
        UserManager<IdentityUser> userManager)
    {
        _siteUserRepository = siteUserRepository;
        _userManager = userManager;
    }

    public async Task<IEnumerable<UserDto>> Handle(GetSiteUsersQuery request, CancellationToken cancellationToken)
    {
        var siteUsers = await _siteUserRepository.FindAsync(su => su.SiteId == request.SiteId);
        var userDtos = new List<UserDto>();

        foreach (var siteUser in siteUsers)
        {
            var user = await _userManager.FindByIdAsync(siteUser.UserId.ToString());
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = Guid.Parse(user.Id),
                    Email = user.Email!,
                    Roles = roles.ToList(),
                    IsActive = true
                });
            }
        }

        return userDtos;
    }
}

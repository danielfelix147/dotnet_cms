using MediatR;
using CMS.Application.Mappings;
using CMS.Application.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CMS.Application.Features.Users.Queries;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
{
    private readonly UserManager<IdentityUser> _userManager;
    

    public GetAllUsersQueryHandler(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
        
    }

    public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userManager.Users.ToListAsync(cancellationToken);
        var userDtos = new List<UserDto>();

        foreach (var user in users)
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

        return userDtos;
    }
}

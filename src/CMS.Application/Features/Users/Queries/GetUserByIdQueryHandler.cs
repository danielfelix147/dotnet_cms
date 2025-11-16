using MediatR;
using CMS.Application.Mappings;
using CMS.Application.DTOs;
using Microsoft.AspNetCore.Identity;

namespace CMS.Application.Features.Users.Queries;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly UserManager<IdentityUser> _userManager;
    

    public GetUserByIdQueryHandler(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
        
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user == null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = Guid.Parse(user.Id),
            Email = user.Email!,
            Roles = roles.ToList(),
            IsActive = true
        };
    }
}

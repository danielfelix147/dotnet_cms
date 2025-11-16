using MediatR;
using CMS.Application.Mappings;
using CMS.Application.DTOs;
using Microsoft.AspNetCore.Identity;

namespace CMS.Application.Features.Users.Commands;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto?>
{
    private readonly UserManager<IdentityUser> _userManager;
    

    public UpdateUserCommandHandler(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
        
    }

    public async Task<UserDto?> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user == null)
            return null;

        user.Email = request.Email;
        user.UserName = request.Email;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return null;

        // Update roles
        if (request.Roles.Any())
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRolesAsync(user, request.Roles);
        }

        var updatedRoles = await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = Guid.Parse(user.Id),
            Email = user.Email!,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Roles = updatedRoles.ToList(),
            IsActive = true
        };
    }
}

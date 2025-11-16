using MediatR;
using CMS.Application.Mappings;
using CMS.Application.DTOs;
using Microsoft.AspNetCore.Identity;

namespace CMS.Application.Features.Users.Commands;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto?>
{
    private readonly UserManager<IdentityUser> _userManager;
    

    public CreateUserCommandHandler(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
        
    }

    public async Task<UserDto?> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return null;

        var user = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return null;

        // Assign roles
        if (request.Roles.Any())
        {
            await _userManager.AddToRolesAsync(user, request.Roles);
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = Guid.Parse(user.Id),
            Email = user.Email!,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Roles = roles.ToList(),
            IsActive = true
        };
    }
}

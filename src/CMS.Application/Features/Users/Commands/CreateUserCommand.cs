using MediatR;
using CMS.Application.DTOs;

namespace CMS.Application.Features.Users.Commands;

public class CreateUserCommand : IRequest<UserDto?>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new() { "Editor" };
}

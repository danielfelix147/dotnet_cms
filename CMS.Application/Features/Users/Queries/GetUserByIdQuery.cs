using MediatR;
using CMS.Application.DTOs;

namespace CMS.Application.Features.Users.Queries;

public class GetUserByIdQuery : IRequest<UserDto?>
{
    public Guid Id { get; set; }
}

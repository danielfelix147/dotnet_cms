using MediatR;
using CMS.Application.DTOs;

namespace CMS.Application.Features.Users.Queries;

public class GetAllUsersQuery : IRequest<IEnumerable<UserDto>>
{
}

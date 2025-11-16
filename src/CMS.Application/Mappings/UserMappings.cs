using CMS.Application.DTOs;
using Microsoft.AspNetCore.Identity;

namespace CMS.Application.Mappings;

public static class UserMappings
{
    public static UserDto ToDto(this IdentityUser user, IList<string>? roles = null)
    {
        return new UserDto
        {
            Id = Guid.Parse(user.Id),
            Email = user.Email!,
            Roles = roles?.ToList() ?? new List<string>(),
            IsActive = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd < DateTimeOffset.UtcNow
        };
    }

    public static List<UserDto> ToDto(this IEnumerable<IdentityUser> users)
    {
        return users.Select(u => u.ToDto()).ToList();
    }
}

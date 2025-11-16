using CMS.Application.DTOs;
using Microsoft.AspNetCore.Identity;

namespace CMS.Application.Interfaces;

public interface IAuthService
{
    Task<TokenResponse?> LoginAsync(LoginRequest request);
    Task<TokenResponse?> RegisterAsync(RegisterRequest request);
    Task<TokenResponse?> RefreshTokenAsync(RefreshTokenRequest request);
    Task<bool> RevokeTokenAsync(string userId);
    Task<IdentityResult> AssignRoleAsync(string userId, string role);
    Task<IList<string>> GetUserRolesAsync(string userId);
}

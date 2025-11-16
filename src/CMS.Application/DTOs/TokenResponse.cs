namespace CMS.Application.DTOs;

public class TokenResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

namespace CMS.Domain.Entities;

public class PasswordResetToken : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty; // Store hashed token
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
}

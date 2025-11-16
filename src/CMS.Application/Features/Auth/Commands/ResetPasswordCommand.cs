using MediatR;
using System.ComponentModel.DataAnnotations;

namespace CMS.Application.Features.Auth.Commands;

public class ResetPasswordCommand : IRequest<ResetPasswordResult>
{
    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$", 
        ErrorMessage = "Password must contain uppercase, lowercase, number, and special character")]
    public string NewPassword { get; set; } = string.Empty;
}

public class ResetPasswordResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

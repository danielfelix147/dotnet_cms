using MediatR;
using System.ComponentModel.DataAnnotations;

namespace CMS.Application.Features.Auth.Commands;

public class ForgotPasswordCommand : IRequest<ForgotPasswordResult>
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
}

public class ForgotPasswordResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

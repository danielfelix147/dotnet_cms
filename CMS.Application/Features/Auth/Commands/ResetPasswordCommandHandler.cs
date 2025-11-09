using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using System.Security.Cryptography;

namespace CMS.Application.Features.Auth.Commands;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResult>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IRepository<PasswordResetToken> _tokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordCommandHandler(
        UserManager<IdentityUser> userManager,
        IRepository<PasswordResetToken> tokenRepository,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _tokenRepository = tokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResetPasswordResult> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Hash the provided token to match against stored hashed tokens
        var hashedToken = Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(request.Token)));

        // Find the reset token
        var resetTokens = await _tokenRepository.GetAllAsync();
        var resetToken = resetTokens.FirstOrDefault(t => t.Token == hashedToken);

        if (resetToken == null)
        {
            return new ResetPasswordResult
            {
                Success = false,
                Message = "Invalid or expired reset token."
            };
        }

        // Validate token is not expired
        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            return new ResetPasswordResult
            {
                Success = false,
                Message = "Reset token has expired. Please request a new password reset."
            };
        }

        // Validate token hasn't been used
        if (resetToken.IsUsed)
        {
            return new ResetPasswordResult
            {
                Success = false,
                Message = "Reset token has already been used. Please request a new password reset."
            };
        }

        // Find the user
        var user = await _userManager.FindByIdAsync(resetToken.UserId);
        if (user == null)
        {
            return new ResetPasswordResult
            {
                Success = false,
                Message = "User not found."
            };
        }

        // Remove old password and set new one
        var removePasswordResult = await _userManager.RemovePasswordAsync(user);
        if (!removePasswordResult.Succeeded)
        {
            return new ResetPasswordResult
            {
                Success = false,
                Message = "Failed to reset password. Please try again."
            };
        }

        var addPasswordResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
        if (!addPasswordResult.Succeeded)
        {
            var errors = string.Join(", ", addPasswordResult.Errors.Select(e => e.Description));
            return new ResetPasswordResult
            {
                Success = false,
                Message = $"Failed to set new password: {errors}"
            };
        }

        // Mark token as used
        resetToken.IsUsed = true;
        resetToken.UpdatedAt = DateTime.UtcNow;
        await _tokenRepository.UpdateAsync(resetToken);
        await _unitOfWork.SaveChangesAsync();

        Console.WriteLine($"=== PASSWORD RESET SUCCESSFUL ===");
        Console.WriteLine($"User: {user.Email}");
        Console.WriteLine($"Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine("=================================");

        return new ResetPasswordResult
        {
            Success = true,
            Message = "Password has been successfully reset. You can now log in with your new password."
        };
    }
}

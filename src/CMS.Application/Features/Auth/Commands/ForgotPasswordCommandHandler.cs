using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using CMS.Application.Services;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using System.Security.Cryptography;

namespace CMS.Application.Features.Auth.Commands;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResult>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IRepository<PasswordResetToken> _tokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
        UserManager<IdentityUser> userManager,
        IRepository<PasswordResetToken> tokenRepository,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        IEmailService emailService)
    {
        _userManager = userManager;
        _tokenRepository = tokenRepository;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<ForgotPasswordResult> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);
        
        // Always return success to prevent email enumeration attacks
        if (user == null)
        {
            return new ForgotPasswordResult
            {
                Success = true,
                Message = "If the email exists, a password reset link has been sent."
            };
        }

        // Generate secure random token (32 bytes = 256 bits)
        var tokenBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenBytes);
        }
        var plainToken = Convert.ToBase64String(tokenBytes);

        // Hash the token for storage (using SHA256)
        var hashedToken = Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(plainToken)));

        // Create password reset token
        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _tokenRepository.AddAsync(resetToken);
        await _unitOfWork.SaveChangesAsync();

        // Get reset URL from configuration
        var resetBaseUrl = _configuration["EmailSettings:ResetPasswordUrl"] ?? "http://localhost:3000/reset-password";

        // Send password reset email
        try
        {
            await _emailService.SendPasswordResetEmailAsync(request.Email, plainToken, resetBaseUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to send email, but token was created: {ex.Message}");
            // Still return success to prevent email enumeration
        }

        return new ForgotPasswordResult
        {
            Success = true,
            Message = "If the email exists, a password reset link has been sent."
        };
    }
}

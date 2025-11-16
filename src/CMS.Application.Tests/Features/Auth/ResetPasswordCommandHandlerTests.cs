using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using CMS.Application.Features.Auth.Commands;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;

namespace CMS.Application.Tests.Features.Auth;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
    private readonly Mock<IRepository<PasswordResetToken>> _mockTokenRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        var store = new Mock<IUserStore<IdentityUser>>();
        _mockUserManager = new Mock<UserManager<IdentityUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _mockTokenRepository = new Mock<IRepository<PasswordResetToken>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _handler = new ResetPasswordCommandHandler(
            _mockUserManager.Object,
            _mockTokenRepository.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldResetPassword()
    {
        // Arrange
        var plainToken = "valid-token-123";
        var hashedToken = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(plainToken)));

        var command = new ResetPasswordCommand
        {
            Token = plainToken,
            NewPassword = "NewPassword@123"
        };

        var user = new IdentityUser { Id = "user-123", Email = "test@example.com" };
        
        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            IsUsed = false
        };

        _mockTokenRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<PasswordResetToken> { resetToken });

        _mockUserManager.Setup(um => um.FindByIdAsync(user.Id))
            .ReturnsAsync(user);

        _mockUserManager.Setup(um => um.RemovePasswordAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(um => um.AddPasswordAsync(user, command.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        _mockTokenRepository.Setup(r => r.UpdateAsync(It.IsAny<PasswordResetToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("successfully reset", result.Message);
        
        _mockUserManager.Verify(um => um.RemovePasswordAsync(user), Times.Once);
        _mockUserManager.Verify(um => um.AddPasswordAsync(user, command.NewPassword), Times.Once);
        _mockTokenRepository.Verify(r => r.UpdateAsync(It.Is<PasswordResetToken>(t => t.IsUsed)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidToken_ShouldReturnFailure()
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Token = "invalid-token",
            NewPassword = "NewPassword@123"
        };

        _mockTokenRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<PasswordResetToken>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid or expired", result.Message);
        
        _mockUserManager.Verify(um => um.RemovePasswordAsync(It.IsAny<IdentityUser>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ShouldReturnFailure()
    {
        // Arrange
        var plainToken = "expired-token";
        var hashedToken = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(plainToken)));

        var command = new ResetPasswordCommand
        {
            Token = plainToken,
            NewPassword = "NewPassword@123"
        };

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = "user-123",
            Token = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddHours(-1), // Expired 1 hour ago
            IsUsed = false
        };

        _mockTokenRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<PasswordResetToken> { resetToken });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("expired", result.Message);
        
        _mockUserManager.Verify(um => um.RemovePasswordAsync(It.IsAny<IdentityUser>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithAlreadyUsedToken_ShouldReturnFailure()
    {
        // Arrange
        var plainToken = "used-token";
        var hashedToken = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(plainToken)));

        var command = new ResetPasswordCommand
        {
            Token = plainToken,
            NewPassword = "NewPassword@123"
        };

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = "user-123",
            Token = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            IsUsed = true // Already used
        };

        _mockTokenRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<PasswordResetToken> { resetToken });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("already been used", result.Message);
        
        _mockUserManager.Verify(um => um.RemovePasswordAsync(It.IsAny<IdentityUser>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var plainToken = "valid-token";
        var hashedToken = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(plainToken)));

        var command = new ResetPasswordCommand
        {
            Token = plainToken,
            NewPassword = "NewPassword@123"
        };

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = "non-existent-user",
            Token = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            IsUsed = false
        };

        _mockTokenRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<PasswordResetToken> { resetToken });

        _mockUserManager.Setup(um => um.FindByIdAsync(resetToken.UserId))
            .ReturnsAsync((IdentityUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User not found", result.Message);
        
        _mockUserManager.Verify(um => um.RemovePasswordAsync(It.IsAny<IdentityUser>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPasswordUpdateFails_ShouldReturnFailure()
    {
        // Arrange
        var plainToken = "valid-token";
        var hashedToken = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(plainToken)));

        var command = new ResetPasswordCommand
        {
            Token = plainToken,
            NewPassword = "weak"
        };

        var user = new IdentityUser { Id = "user-123", Email = "test@example.com" };
        
        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            IsUsed = false
        };

        _mockTokenRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<PasswordResetToken> { resetToken });

        _mockUserManager.Setup(um => um.FindByIdAsync(user.Id))
            .ReturnsAsync(user);

        _mockUserManager.Setup(um => um.RemovePasswordAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(um => um.AddPasswordAsync(user, command.NewPassword))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Description = "Password is too weak" }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Password is too weak", result.Message);
        
        // Token should NOT be marked as used if password update fails
        _mockTokenRepository.Verify(r => r.UpdateAsync(It.IsAny<PasswordResetToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldMarkTokenAsUsed_OnSuccess()
    {
        // Arrange
        var plainToken = "valid-token";
        var hashedToken = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(plainToken)));

        var command = new ResetPasswordCommand
        {
            Token = plainToken,
            NewPassword = "NewPassword@123"
        };

        var user = new IdentityUser { Id = "user-123" };
        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            IsUsed = false
        };

        PasswordResetToken? updatedToken = null;

        _mockTokenRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<PasswordResetToken> { resetToken });

        _mockUserManager.Setup(um => um.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _mockUserManager.Setup(um => um.RemovePasswordAsync(user)).ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(um => um.AddPasswordAsync(user, command.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        _mockTokenRepository.Setup(r => r.UpdateAsync(It.IsAny<PasswordResetToken>()))
            .Callback<PasswordResetToken>(t => updatedToken = t)
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(updatedToken);
        Assert.True(updatedToken.IsUsed);
        Assert.NotNull(updatedToken.UpdatedAt);
    }
}

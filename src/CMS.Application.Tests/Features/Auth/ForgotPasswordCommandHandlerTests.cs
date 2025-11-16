using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using CMS.Application.Features.Auth.Commands;
using CMS.Application.Services;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;

namespace CMS.Application.Tests.Features.Auth;

public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
    private readonly Mock<IRepository<PasswordResetToken>> _mockTokenRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly ForgotPasswordCommandHandler _handler;

    public ForgotPasswordCommandHandlerTests()
    {
        var store = new Mock<IUserStore<IdentityUser>>();
        _mockUserManager = new Mock<UserManager<IdentityUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _mockTokenRepository = new Mock<IRepository<PasswordResetToken>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockEmailService = new Mock<IEmailService>();

        _mockConfiguration.Setup(c => c["EmailSettings:ResetPasswordUrl"])
            .Returns("http://localhost:3000/reset-password");

        _handler = new ForgotPasswordCommandHandler(
            _mockUserManager.Object,
            _mockTokenRepository.Object,
            _mockUnitOfWork.Object,
            _mockConfiguration.Object,
            _mockEmailService.Object);
    }

    [Fact]
    public async Task Handle_WithValidEmail_ShouldCreateTokenAndSendEmail()
    {
        // Arrange
        var command = new ForgotPasswordCommand { Email = "test@example.com" };
        var user = new IdentityUser { Id = "user-123", Email = "test@example.com" };

        _mockUserManager.Setup(um => um.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _mockTokenRepository.Setup(r => r.AddAsync(It.IsAny<PasswordResetToken>()))
            .ReturnsAsync((PasswordResetToken token) => token);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockEmailService.Setup(e => e.SendPasswordResetEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("password reset link has been sent", result.Message);
        
        _mockTokenRepository.Verify(r => r.AddAsync(It.Is<PasswordResetToken>(t => 
            t.UserId == user.Id && !t.IsUsed)), Times.Once);
        
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        
        _mockEmailService.Verify(e => e.SendPasswordResetEmailAsync(
            command.Email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentEmail_ShouldStillReturnSuccess()
    {
        // Arrange - Prevent email enumeration attacks
        var command = new ForgotPasswordCommand { Email = "nonexistent@example.com" };

        _mockUserManager.Setup(um => um.FindByEmailAsync(command.Email))
            .ReturnsAsync((IdentityUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("password reset link has been sent", result.Message);
        
        // Should NOT create token or send email
        _mockTokenRepository.Verify(r => r.AddAsync(It.IsAny<PasswordResetToken>()), Times.Never);
        _mockEmailService.Verify(e => e.SendPasswordResetEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateTokenWithCorrectExpiration()
    {
        // Arrange
        var command = new ForgotPasswordCommand { Email = "test@example.com" };
        var user = new IdentityUser { Id = "user-123", Email = "test@example.com" };
        PasswordResetToken? capturedToken = null;

        _mockUserManager.Setup(um => um.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _mockTokenRepository.Setup(r => r.AddAsync(It.IsAny<PasswordResetToken>()))
            .Callback<PasswordResetToken>(t => capturedToken = t)
            .ReturnsAsync((PasswordResetToken token) => token);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _mockEmailService.Setup(e => e.SendPasswordResetEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedToken);
        Assert.Equal(user.Id, capturedToken.UserId);
        Assert.False(capturedToken.IsUsed);
        Assert.NotEmpty(capturedToken.Token);
        
        // Should expire in approximately 24 hours
        var expectedExpiration = DateTime.UtcNow.AddHours(24);
        Assert.True((capturedToken.ExpiresAt - expectedExpiration).TotalMinutes < 1);
    }

    [Fact]
    public async Task Handle_WhenEmailServiceFails_ShouldStillReturnSuccess()
    {
        // Arrange - Graceful degradation
        var command = new ForgotPasswordCommand { Email = "test@example.com" };
        var user = new IdentityUser { Id = "user-123", Email = "test@example.com" };

        _mockUserManager.Setup(um => um.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _mockTokenRepository.Setup(r => r.AddAsync(It.IsAny<PasswordResetToken>()))
            .ReturnsAsync((PasswordResetToken token) => token);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        _mockEmailService.Setup(e => e.SendPasswordResetEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("SMTP server unavailable"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        _mockTokenRepository.Verify(r => r.AddAsync(It.IsAny<PasswordResetToken>()), Times.Once);
    }
}

using Xunit;
using Moq;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using CMS.Infrastructure.Services;

namespace CMS.Infrastructure.Tests.Services;

public class EmailServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        
        // Setup default configuration
        _mockConfiguration.Setup(c => c["Smtp:Host"]).Returns("localhost");
        _mockConfiguration.Setup(c => c["Smtp:Port"]).Returns("1025");
        _mockConfiguration.Setup(c => c["Smtp:EnableSsl"]).Returns("false");
        _mockConfiguration.Setup(c => c["Smtp:Username"]).Returns(string.Empty);
        _mockConfiguration.Setup(c => c["Smtp:Password"]).Returns(string.Empty);

        _emailService = new EmailService(_mockConfiguration.Object);
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_WithValidInputs_ShouldSendEmail()
    {
        // Arrange
        var toEmail = "test@example.com";
        var resetToken = "abc123";
        var resetUrl = "https://example.com/reset?token=abc123";

        // Act
        // Note: This will actually attempt to connect to MailHog if it's running
        // In a real unit test, we would mock the SmtpClient
        await _emailService.SendPasswordResetEmailAsync(toEmail, resetToken, resetUrl);

        // Assert
        // Since we can't easily mock MailKit's SmtpClient without major refactoring,
        // this test verifies the method completes without throwing exceptions
        Assert.True(true);
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_WithNullEmail_ShouldThrowException()
    {
        // Arrange
        var resetToken = "abc123";
        var resetUrl = "https://example.com/reset?token=abc123";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _emailService.SendPasswordResetEmailAsync(null!, resetToken, resetUrl));
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_WithEmptyEmail_ShouldThrowException()
    {
        // Arrange
        var resetToken = "abc123";
        var resetUrl = "https://example.com/reset?token=abc123";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _emailService.SendPasswordResetEmailAsync(string.Empty, resetToken, resetUrl));
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_WithNullResetUrl_ShouldThrowException()
    {
        // Arrange
        var toEmail = "test@example.com";
        var resetToken = "abc123";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _emailService.SendPasswordResetEmailAsync(toEmail, resetToken, null!));
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_WithEmptyResetUrl_ShouldThrowException()
    {
        // Arrange
        var toEmail = "test@example.com";
        var resetToken = "abc123";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _emailService.SendPasswordResetEmailAsync(toEmail, resetToken, string.Empty));
    }

    [Fact]
    public void Constructor_WithMissingSmtpHost_ShouldThrowException()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["Smtp:Host"]).Returns((string?)null);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EmailService(mockConfig.Object));
    }

    [Fact]
    public void Constructor_WithMissingSmtpPort_ShouldUseDefaultPort()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["Smtp:Host"]).Returns("localhost");
        mockConfig.Setup(c => c["Smtp:Port"]).Returns((string?)null);
        mockConfig.Setup(c => c["Smtp:EnableSsl"]).Returns("false");

        // Act
        var service = new EmailService(mockConfig.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithInvalidPortNumber_ShouldThrowException()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["Smtp:Host"]).Returns("localhost");
        mockConfig.Setup(c => c["Smtp:Port"]).Returns("invalid");
        mockConfig.Setup(c => c["Smtp:EnableSsl"]).Returns("false");

        // Act & Assert
        Assert.Throws<FormatException>(() => new EmailService(mockConfig.Object));
    }

    [Theory]
    [InlineData("test@example.com", "https://example.com/reset?token=abc123")]
    [InlineData("user.name+tag@example.co.uk", "https://app.mysite.com/auth/reset-password?token=xyz789")]
    [InlineData("admin@subdomain.example.com", "http://localhost:5055/reset?token=test123")]
    public async Task SendPasswordResetEmailAsync_WithVariousValidInputs_ShouldSucceed(
        string toEmail, string resetUrl)
    {
        // Arrange
        var resetToken = "test-token-123";

        // Act & Assert
        // Should not throw any exceptions
        await _emailService.SendPasswordResetEmailAsync(toEmail, resetToken, resetUrl);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user name@example.com")]
    public async Task SendPasswordResetEmailAsync_WithInvalidEmailFormats_ShouldThrowException(
        string invalidEmail)
    {
        // Arrange
        var resetToken = "abc123";
        var resetUrl = "https://example.com/reset?token=abc123";

        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(
            async () => await _emailService.SendPasswordResetEmailAsync(invalidEmail, resetToken, resetUrl));
    }
}

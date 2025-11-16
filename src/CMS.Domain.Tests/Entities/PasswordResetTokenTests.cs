using Xunit;
using CMS.Domain.Entities;

namespace CMS.Domain.Tests.Entities;

public class PasswordResetTokenTests
{
    [Fact]
    public void PasswordResetToken_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = "test-user",
            Token = "test-token"
        };

        // Assert
        Assert.NotEqual(Guid.Empty, token.Id);
        Assert.Equal("test-user", token.UserId);
        Assert.Equal("test-token", token.Token);
        Assert.False(token.IsUsed);
        Assert.False(token.IsDeleted);
    }

    [Fact]
    public void PasswordResetToken_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var userId = "user-123";
        var tokenValue = "hashed-token";
        var expiresAt = DateTime.UtcNow.AddHours(24);

        // Act
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = tokenValue,
            ExpiresAt = expiresAt,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(userId, token.UserId);
        Assert.Equal(tokenValue, token.Token);
        Assert.Equal(expiresAt, token.ExpiresAt);
        Assert.False(token.IsUsed);
    }

    [Fact]
    public void PasswordResetToken_ShouldMarkAsUsed()
    {
        // Arrange
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = "user-123",
            Token = "hashed-token",
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            IsUsed = false
        };

        // Act
        token.IsUsed = true;
        token.UpdatedAt = DateTime.UtcNow;

        // Assert
        Assert.True(token.IsUsed);
        Assert.NotNull(token.UpdatedAt);
    }

    [Fact]
    public void PasswordResetToken_ShouldBeExpired_WhenExpiresAtIsInPast()
    {
        // Arrange
        var token = new PasswordResetToken
        {
            ExpiresAt = DateTime.UtcNow.AddHours(-1) // 1 hour ago
        };

        // Act
        var isExpired = token.ExpiresAt < DateTime.UtcNow;

        // Assert
        Assert.True(isExpired);
    }

    [Fact]
    public void PasswordResetToken_ShouldNotBeExpired_WhenExpiresAtIsInFuture()
    {
        // Arrange
        var token = new PasswordResetToken
        {
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        // Act
        var isExpired = token.ExpiresAt < DateTime.UtcNow;

        // Assert
        Assert.False(isExpired);
    }
}

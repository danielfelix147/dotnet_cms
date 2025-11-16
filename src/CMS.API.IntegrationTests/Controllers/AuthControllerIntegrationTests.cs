using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using CMS.Application.Features.Auth.Commands;
using CMS.Domain.Entities;
using CMS.Infrastructure.Data;

namespace CMS.API.IntegrationTests.Controllers;

public class AuthControllerIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AuthControllerIntegrationTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ForgotPassword_WithValidEmail_ShouldReturnSuccess()
    {
        // Arrange - Create a test user first
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var testEmail = $"test-{Guid.NewGuid()}@example.com";
        
        var user = new IdentityUser
        {
            UserName = testEmail,
            Email = testEmail,
            EmailConfirmed = true
        };
        
        await userManager.CreateAsync(user, "Test@Password123");

        var command = new ForgotPasswordCommand { Email = testEmail };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", command);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent, JsonOptions);
        
        Assert.True(jsonResponse.GetProperty("success").GetBoolean());
        Assert.Contains("password reset link has been sent", jsonResponse.GetProperty("message").GetString());
    }

    [Fact]
    public async Task ForgotPassword_WithNonExistentEmail_ShouldReturnSuccessToPreventEnumeration()
    {
        // Arrange
        var command = new ForgotPasswordCommand 
        { 
            Email = $"nonexistent-{Guid.NewGuid()}@example.com" 
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", command);

        // Assert
        // Should return success to prevent email enumeration attacks
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent, JsonOptions);
        
        Assert.True(jsonResponse.GetProperty("success").GetBoolean());
        Assert.Contains("password reset link has been sent", jsonResponse.GetProperty("message").GetString());
    }

    [Fact]
    public async Task ForgotPassword_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new ForgotPasswordCommand { Email = "invalid-email" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_ShouldResetPassword()
    {
        // Arrange - Create user and reset token
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<CMSDbContext>();
        
        var testEmail = $"reset-test-{Guid.NewGuid()}@example.com";
        var user = new IdentityUser
        {
            UserName = testEmail,
            Email = testEmail,
            EmailConfirmed = true
        };
        
        await userManager.CreateAsync(user, "OldPassword@123");

        var plainToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var hashedToken = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(plainToken)));

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.PasswordResetTokens.Add(resetToken);
        await dbContext.SaveChangesAsync();

        var command = new ResetPasswordCommand
        {
            Token = plainToken,
            NewPassword = "NewP@ssw0rd!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", command);

        // Assert
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Valid token response ({response.StatusCode}): {responseContent}");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseContent, JsonOptions);
        
        Assert.True(jsonResponse.GetProperty("success").GetBoolean());
        Assert.Contains("successfully reset", jsonResponse.GetProperty("message").GetString());
    }

    [Fact]
    public async Task ResetPassword_WithExpiredToken_ShouldReturnFailure()
    {
        // Arrange - Create user and expired reset token
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<CMSDbContext>();
        
        var testEmail = $"expired-test-{Guid.NewGuid()}@example.com";
        var user = new IdentityUser
        {
            UserName = testEmail,
            Email = testEmail,
            EmailConfirmed = true
        };
        
        await userManager.CreateAsync(user, "OldPassword@123");

        var plainToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var hashedToken = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(plainToken)));

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddHours(-1), // Expired 1 hour ago
            IsUsed = false,
            CreatedAt = DateTime.UtcNow.AddHours(-25)
        };

        dbContext.PasswordResetTokens.Add(resetToken);
        await dbContext.SaveChangesAsync();

        var command = new ResetPasswordCommand
        {
            Token = plainToken,
            NewPassword = "NewP@ssw0rd!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", command);

        // Assert - Should return BadRequest for expired tokens
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_ShouldReturnFailure()
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Token = "totally-invalid-token",
            NewPassword = "NewP@ssw0rd!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithAlreadyUsedToken_ShouldReturnFailure()
    {
        // Arrange - Create user and used reset token
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<CMSDbContext>();
        
        var testEmail = $"used-token-test-{Guid.NewGuid()}@example.com";
        var user = new IdentityUser
        {
            UserName = testEmail,
            Email = testEmail,
            EmailConfirmed = true
        };
        
        await userManager.CreateAsync(user, "OldPassword@123");

        var plainToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var hashedToken = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(plainToken)));

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            IsUsed = true, // Already used
            CreatedAt = DateTime.UtcNow
        };

        dbContext.PasswordResetTokens.Add(resetToken);
        await dbContext.SaveChangesAsync();

        var command = new ResetPasswordCommand
        {
            Token = plainToken,
            NewPassword = "NewP@ssw0rd!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", command);

        // Assert - Should return BadRequest for already used tokens
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithWeakPassword_ShouldReturnValidationError()
    {
        // Arrange - Create user and valid reset token
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<CMSDbContext>();
        
        var testEmail = $"weak-password-test-{Guid.NewGuid()}@example.com";
        var user = new IdentityUser
        {
            UserName = testEmail,
            Email = testEmail,
            EmailConfirmed = true
        };
        
        await userManager.CreateAsync(user, "OldPassword@123");

        var plainToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var hashedToken = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(plainToken)));

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.PasswordResetTokens.Add(resetToken);
        await dbContext.SaveChangesAsync();

        var command = new ResetPasswordCommand
        {
            Token = plainToken,
            NewPassword = "weak" // Too weak
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

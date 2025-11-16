#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type

using CMS.Application.DTOs;
using CMS.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CMS.Infrastructure.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        var contextAccessorMock = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
        _signInManagerMock = new Mock<SignInManager<IdentityUser>>(
            _userManagerMock.Object, contextAccessorMock.Object, userPrincipalFactoryMock.Object, null, null, null, null);

        _configurationMock = new Mock<IConfiguration>();
        SetupConfiguration();

        _authService = new AuthService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _configurationMock.Object);
    }

    private void SetupConfiguration()
    {
        var jwtSettingsSection = new Mock<IConfigurationSection>();
        jwtSettingsSection.Setup(x => x["SecretKey"]).Returns("ThisIsAVerySecureSecretKeyForJwtTokenGeneration12345");
        jwtSettingsSection.Setup(x => x["Issuer"]).Returns("CMSTestIssuer");
        jwtSettingsSection.Setup(x => x["Audience"]).Returns("CMSTestAudience");

        _configurationMock.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSettingsSection.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnTokenResponse()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";
        var user = new IdentityUser { Id = Guid.NewGuid().ToString(), Email = email, UserName = email };
        var loginRequest = new LoginRequest { Email = email, Password = password };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, password, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Editor" });

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Email.Should().Be(email);
        result.User.Roles.Should().Contain("Editor");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldReturnNull()
    {
        // Arrange
        var loginRequest = new LoginRequest { Email = "nonexistent@example.com", Password = "Password123!" };
        _userManagerMock.Setup(x => x.FindByEmailAsync(loginRequest.Email)).ReturnsAsync((IdentityUser?)null);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnNull()
    {
        // Arrange
        var email = "test@example.com";
        var user = new IdentityUser { Id = Guid.NewGuid().ToString(), Email = email, UserName = email };
        var loginRequest = new LoginRequest { Email = email, Password = "WrongPassword!" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, loginRequest.Password, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        // Act
        var result = await _authService.LoginAsync(loginRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_WithNewUser_ShouldReturnTokenResponse()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "User"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(registerRequest.Email)).ReturnsAsync((IdentityUser?)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), registerRequest.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), "Editor"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(new List<string> { "Editor" });

        // Act
        var result = await _authService.RegisterAsync(registerRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.User.Email.Should().Be(registerRequest.Email);
        result.User.Roles.Should().Contain("Editor");
        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<IdentityUser>(), registerRequest.Password), Times.Once);
        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), "Editor"), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUser_ShouldReturnNull()
    {
        // Arrange
        var email = "existing@example.com";
        var existingUser = new IdentityUser { Id = Guid.NewGuid().ToString(), Email = email, UserName = email };
        var registerRequest = new RegisterRequest
        {
            Email = email,
            Password = "Password123!",
            FirstName = "Existing",
            LastName = "User"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(existingUser);

        // Act
        var result = await _authService.RegisterAsync(registerRequest);

        // Assert
        result.Should().BeNull();
        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WhenCreateFails_ShouldReturnNull()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "Password123!",
            FirstName = "Failed",
            LastName = "User"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(registerRequest.Email)).ReturnsAsync((IdentityUser?)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), registerRequest.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

        // Act
        var result = await _authService.RegisterAsync(registerRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AssignRoleAsync_WithValidUser_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new IdentityUser { Id = userId, Email = "test@example.com", UserName = "test@example.com" };
        var newRole = "Admin";

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Editor" });
        _userManagerMock.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(user, newRole)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.AssignRoleAsync(userId, newRole);

        // Assert
        result.Succeeded.Should().BeTrue();
        _userManagerMock.Verify(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()), Times.Once);
        _userManagerMock.Verify(x => x.AddToRoleAsync(user, newRole), Times.Once);
    }

    [Fact]
    public async Task AssignRoleAsync_WithInvalidUser_ShouldFail()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((IdentityUser?)null);

        // Act
        var result = await _authService.AssignRoleAsync(userId, "Admin");

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Description == "User not found");
    }

    [Fact]
    public async Task GetUserRolesAsync_WithValidUser_ShouldReturnRoles()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new IdentityUser { Id = userId, Email = "test@example.com", UserName = "test@example.com" };
        var roles = new List<string> { "Admin", "Editor" };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(roles);

        // Act
        var result = await _authService.GetUserRolesAsync(userId);

        // Assert
        result.Should().BeEquivalentTo(roles);
    }

    [Fact]
    public async Task GetUserRolesAsync_WithInvalidUser_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((IdentityUser?)null);

        // Act
        var result = await _authService.GetUserRolesAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task RevokeTokenAsync_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        // Act
        var result = await _authService.RevokeTokenAsync(userId);

        // Assert
        result.Should().BeTrue();
    }
}

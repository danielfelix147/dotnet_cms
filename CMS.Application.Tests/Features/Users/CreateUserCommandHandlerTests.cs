#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type

using CMS.Application.DTOs;
using CMS.Application.Features.Users.Commands;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace CMS.Application.Tests.Features.Users;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);
        _handler = new CreateUserCommandHandler(_userManagerMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_User_Successfully()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "Test@123",
            FirstName = "John",
            LastName = "Doe",
            Roles = new List<string> { "Editor" }
        };

        _userManagerMock.Setup(um => um.FindByEmailAsync(command.Email))
            .ReturnsAsync((IdentityUser?)null);
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.AddToRolesAsync(It.IsAny<IdentityUser>(), command.Roles))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(command.Roles);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(command.Email);
        result.FirstName.Should().Be(command.FirstName);
        result.LastName.Should().Be(command.LastName);
        result.Roles.Should().Contain("Editor");
        _userManagerMock.Verify(um => um.CreateAsync(It.IsAny<IdentityUser>(), command.Password), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_User_Already_Exists()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "existing@example.com",
            Password = "Test@123"
        };

        var existingUser = new IdentityUser { Email = command.Email };
        _userManagerMock.Setup(um => um.FindByEmailAsync(command.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _userManagerMock.Verify(um => um.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Creation_Fails()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "weak"
        };

        _userManagerMock.Setup(um => um.FindByEmailAsync(command.Email))
            .ReturnsAsync((IdentityUser?)null);
        _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}

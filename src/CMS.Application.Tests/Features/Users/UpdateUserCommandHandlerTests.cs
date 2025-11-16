#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type

using CMS.Application.Features.Users.Commands;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace CMS.Application.Tests.Features.Users;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);
        _handler = new UpdateUserCommandHandler(_userManagerMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_User_Successfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new IdentityUser { Id = userId.ToString(), Email = "old@example.com" };
        var command = new UpdateUserCommand
        {
            Id = userId,
            Email = "new@example.com",
            FirstName = "Jane",
            LastName = "Smith",
            Roles = new List<string> { "Admin" }
        };

        _userManagerMock.Setup(um => um.FindByIdAsync(userId.ToString())).ReturnsAsync(existingUser);
        _userManagerMock.Setup(um => um.UpdateAsync(It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<IdentityUser>())).ReturnsAsync(new List<string> { "Editor" });
        _userManagerMock.Setup(um => um.RemoveFromRolesAsync(It.IsAny<IdentityUser>(), It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(um => um.AddToRolesAsync(It.IsAny<IdentityUser>(), command.Roles)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.SetupSequence(um => um.GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(new List<string> { "Editor" })
            .ReturnsAsync(command.Roles);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(command.Email);
        _userManagerMock.Verify(um => um.UpdateAsync(It.IsAny<IdentityUser>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_User_Not_Found()
    {
        // Arrange
        var command = new UpdateUserCommand { Id = Guid.NewGuid(), Email = "test@example.com" };
        _userManagerMock.Setup(um => um.FindByIdAsync(command.Id.ToString())).ReturnsAsync((IdentityUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}

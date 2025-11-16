#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type

using CMS.Application.Features.Users.Commands;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace CMS.Application.Tests.Features.Users;

public class DeleteUserCommandHandlerTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly DeleteUserCommandHandler _handler;

    public DeleteUserCommandHandlerTests()
    {
        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);
        _handler = new DeleteUserCommandHandler(_userManagerMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Delete_User_Successfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new IdentityUser { Id = userId.ToString(), Email = "test@example.com" };
        var command = new DeleteUserCommand { Id = userId };

        _userManagerMock.Setup(um => um.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(um => um.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _userManagerMock.Verify(um => um.DeleteAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_False_When_User_Not_Found()
    {
        // Arrange
        var command = new DeleteUserCommand { Id = Guid.NewGuid() };
        _userManagerMock.Setup(um => um.FindByIdAsync(command.Id.ToString())).ReturnsAsync((IdentityUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Return_False_When_Delete_Fails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new IdentityUser { Id = userId.ToString() };
        var command = new DeleteUserCommand { Id = userId };

        _userManagerMock.Setup(um => um.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(um => um.DeleteAsync(user)).ReturnsAsync(IdentityResult.Failed());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }
}

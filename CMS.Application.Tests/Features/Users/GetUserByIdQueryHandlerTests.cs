#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type

using CMS.Application.Features.Users.Queries;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace CMS.Application.Tests.Features.Users;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);
        _handler = new GetUserByIdQueryHandler(_userManagerMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_User_When_Found()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new IdentityUser { Id = userId.ToString(), Email = "test@example.com" };
        var query = new GetUserByIdQuery { Id = userId };

        _userManagerMock.Setup(um => um.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Email.Should().Be("test@example.com");
        result.Roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_User_Not_Found()
    {
        // Arrange
        var query = new GetUserByIdQuery { Id = Guid.NewGuid() };
        _userManagerMock.Setup(um => um.FindByIdAsync(query.Id.ToString())).ReturnsAsync((IdentityUser?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}

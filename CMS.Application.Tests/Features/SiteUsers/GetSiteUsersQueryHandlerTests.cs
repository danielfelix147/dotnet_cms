using CMS.Application.Features.SiteUsers.Queries;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Linq.Expressions;

namespace CMS.Application.Tests.Features.SiteUsers;

public class GetSiteUsersQueryHandlerTests
{
    private readonly Mock<IRepository<SiteUser>> _siteUserRepositoryMock;
    private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
    private readonly GetSiteUsersQueryHandler _handler;

    public GetSiteUsersQueryHandlerTests()
    {
        _siteUserRepositoryMock = new Mock<IRepository<SiteUser>>();
        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        _userManagerMock = new Mock<UserManager<IdentityUser>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _handler = new GetSiteUsersQueryHandler(_siteUserRepositoryMock.Object, _userManagerMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Users_For_Site()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();
        
        var siteUsers = new List<SiteUser>
        {
            new SiteUser { Id = Guid.NewGuid(), SiteId = siteId, UserId = userId1, Role = "Editor" },
            new SiteUser { Id = Guid.NewGuid(), SiteId = siteId, UserId = userId2, Role = "Viewer" }
        };

        var query = new GetSiteUsersQuery { SiteId = siteId };

        _siteUserRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<SiteUser, bool>>>()))
            .ReturnsAsync(siteUsers);
        _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => new IdentityUser { Id = id, Email = $"user{id}@example.com" });
        _userManagerMock.Setup(um => um.GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(new List<string> { "User" });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }
}

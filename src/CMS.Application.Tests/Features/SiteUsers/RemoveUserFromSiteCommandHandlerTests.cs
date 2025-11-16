using CMS.Application.Features.SiteUsers.Commands;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;

namespace CMS.Application.Tests.Features.SiteUsers;

public class RemoveUserFromSiteCommandHandlerTests
{
    private readonly Mock<IRepository<SiteUser>> _siteUserRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RemoveUserFromSiteCommandHandler _handler;

    public RemoveUserFromSiteCommandHandlerTests()
    {
        _siteUserRepositoryMock = new Mock<IRepository<SiteUser>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new RemoveUserFromSiteCommandHandler(_siteUserRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Remove_User_From_Site_Successfully()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var command = new RemoveUserFromSiteCommand { SiteId = siteId, UserId = userId };

        var siteUser = new SiteUser { Id = Guid.NewGuid(), SiteId = siteId, UserId = userId };
        _siteUserRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<SiteUser, bool>>>()))
            .ReturnsAsync(new List<SiteUser> { siteUser });
        _siteUserRepositoryMock.Setup(r => r.DeleteAsync(siteUser)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _siteUserRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<SiteUser>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_False_When_Assignment_Not_Found()
    {
        // Arrange
        var command = new RemoveUserFromSiteCommand { SiteId = Guid.NewGuid(), UserId = Guid.NewGuid().ToString() };
        _siteUserRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<SiteUser, bool>>>()))
            .ReturnsAsync(new List<SiteUser>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }
}

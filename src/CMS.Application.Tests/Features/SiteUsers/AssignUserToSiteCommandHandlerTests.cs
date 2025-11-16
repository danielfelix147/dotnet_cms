using CMS.Application.Features.SiteUsers.Commands;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;

namespace CMS.Application.Tests.Features.SiteUsers;

public class AssignUserToSiteCommandHandlerTests
{
    private readonly Mock<IRepository<SiteUser>> _siteUserRepositoryMock;
    private readonly Mock<IRepository<Site>> _siteRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AssignUserToSiteCommandHandler _handler;

    public AssignUserToSiteCommandHandlerTests()
    {
        _siteUserRepositoryMock = new Mock<IRepository<SiteUser>>();
        _siteRepositoryMock = new Mock<IRepository<Site>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new AssignUserToSiteCommandHandler(
            _siteUserRepositoryMock.Object,
            _siteRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Assign_User_To_Site_Successfully()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new AssignUserToSiteCommand
        {
            SiteId = siteId,
            UserId = userId.ToString(),
            Role = "Editor"
        };

        _siteRepositoryMock.Setup(r => r.GetByIdAsync(siteId)).ReturnsAsync(new Site { Id = siteId });
        _siteUserRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<SiteUser, bool>>>()))
            .ReturnsAsync(new List<SiteUser>());
        _siteUserRepositoryMock.Setup(r => r.AddAsync(It.IsAny<SiteUser>())).ReturnsAsync((SiteUser su) => su);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _siteUserRepositoryMock.Verify(r => r.AddAsync(It.IsAny<SiteUser>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_False_When_Site_Not_Found()
    {
        // Arrange
        var command = new AssignUserToSiteCommand { SiteId = Guid.NewGuid(), UserId = Guid.NewGuid().ToString() };
        _siteRepositoryMock.Setup(r => r.GetByIdAsync(command.SiteId)).ReturnsAsync((Site?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }
}

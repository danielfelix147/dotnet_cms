using CMS.Application.Features.Sites.Commands;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Sites;

public class DeleteSiteCommandHandlerTests
{
    private readonly Mock<ISiteRepository> _siteRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteSiteCommandHandler _handler;

    public DeleteSiteCommandHandlerTests()
    {
        _siteRepositoryMock = new Mock<ISiteRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new DeleteSiteCommandHandler(_siteRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Soft_Delete_Site_Successfully()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var command = new DeleteSiteCommand { Id = siteId };

        var existingSite = new Site
        {
            Id = siteId,
            Name = "Test Site",
            Domain = "test.com",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        Site? capturedSite = null;

        _siteRepositoryMock.Setup(r => r.GetByIdAsync(siteId)).ReturnsAsync(existingSite);
        _siteRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Site>()))
            .Callback<Site>(s => capturedSite = s)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        capturedSite.Should().NotBeNull();
        capturedSite!.IsDeleted.Should().BeTrue();
        capturedSite.UpdatedAt.Should().NotBeNull();
        capturedSite.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        
        _siteRepositoryMock.Verify(r => r.GetByIdAsync(siteId), Times.Once);
        _siteRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Site>(s => s.IsDeleted)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_False_When_Site_Not_Found()
    {
        // Arrange
        var command = new DeleteSiteCommand { Id = Guid.NewGuid() };

        _siteRepositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Site?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _siteRepositoryMock.Verify(r => r.GetByIdAsync(command.Id), Times.Once);
        _siteRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Site>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Not_Physically_Delete_Site()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var command = new DeleteSiteCommand { Id = siteId };

        var existingSite = new Site
        {
            Id = siteId,
            Name = "Test Site",
            Domain = "test.com",
            IsDeleted = false
        };

        _siteRepositoryMock.Setup(r => r.GetByIdAsync(siteId)).ReturnsAsync(existingSite);
        _siteRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Site>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _siteRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Site>()), Times.Never);
        _siteRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Site>()), Times.Once);
    }
}

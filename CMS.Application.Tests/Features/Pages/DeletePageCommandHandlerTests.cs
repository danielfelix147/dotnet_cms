using CMS.Application.Features.Pages.Commands;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Pages;

public class DeletePageCommandHandlerTests
{
    private readonly Mock<IRepository<Page>> _pageRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeletePageCommandHandler _handler;

    public DeletePageCommandHandlerTests()
    {
        _pageRepositoryMock = new Mock<IRepository<Page>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new DeletePageCommandHandler(_pageRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Delete_Page_Successfully()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var existingPage = new Page
        {
            Id = pageId,
            SiteId = siteId,
            PageId = "test-page",
            Title = "Test Page",
            IsDeleted = false
        };

        var command = new DeletePageCommand { Id = pageId, SiteId = siteId };

        Page deletedPage = null!;
        _pageRepositoryMock.Setup(r => r.GetByIdAsync(pageId)).ReturnsAsync(existingPage);
        _pageRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Page>()))
            .Callback<Page>(p => deletedPage = p)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        deletedPage.Should().NotBeNull();
        deletedPage.IsDeleted.Should().BeTrue();
        deletedPage.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        _pageRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Page>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_False_When_Page_Not_Found()
    {
        // Arrange
        var command = new DeletePageCommand
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid()
        };

        _pageRepositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Page?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _pageRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Page>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_False_When_SiteId_Mismatch()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var existingPage = new Page
        {
            Id = pageId,
            SiteId = Guid.NewGuid(),
            PageId = "page",
            Title = "Page"
        };

        var command = new DeletePageCommand
        {
            Id = pageId,
            SiteId = Guid.NewGuid() // Different SiteId
        };

        _pageRepositoryMock.Setup(r => r.GetByIdAsync(pageId)).ReturnsAsync(existingPage);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _pageRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Page>()), Times.Never);
    }
}

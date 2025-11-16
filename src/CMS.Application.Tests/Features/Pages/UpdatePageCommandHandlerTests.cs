using CMS.Application.Features.Pages.Commands;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Pages;

public class UpdatePageCommandHandlerTests
{
    private readonly Mock<IRepository<Page>> _pageRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdatePageCommandHandler _handler;

    public UpdatePageCommandHandlerTests()
    {
        _pageRepositoryMock = new Mock<IRepository<Page>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdatePageCommandHandler(_pageRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Page_Successfully()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var existingPage = new Page
        {
            Id = pageId,
            SiteId = siteId,
            PageId = "old-page",
            Title = "Old Title",
            Description = "Old Description",
            IsPublished = false
        };

        var command = new UpdatePageCommand
        {
            Id = pageId,
            SiteId = siteId,
            PageId = "new-page",
            Title = "New Title",
            Description = "New Description",
            IsPublished = true
        };

        _pageRepositoryMock.Setup(r => r.GetByIdAsync(pageId)).ReturnsAsync(existingPage);
        _pageRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Page>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.PageId.Should().Be(command.PageId);
        result.Title.Should().Be(command.Title);
        result.Description.Should().Be(command.Description);
        result.IsPublished.Should().Be(command.IsPublished);
        _pageRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Page>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Page_Not_Found()
    {
        // Arrange
        var command = new UpdatePageCommand
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
            PageId = "test",
            Title = "Test"
        };

        _pageRepositoryMock.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Page?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _pageRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Page>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_SiteId_Mismatch()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var existingPage = new Page
        {
            Id = pageId,
            SiteId = Guid.NewGuid(),
            PageId = "page",
            Title = "Title"
        };

        var command = new UpdatePageCommand
        {
            Id = pageId,
            SiteId = Guid.NewGuid(), // Different SiteId
            PageId = "page",
            Title = "Updated Title"
        };

        _pageRepositoryMock.Setup(r => r.GetByIdAsync(pageId)).ReturnsAsync(existingPage);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _pageRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Page>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Update_Timestamp()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var existingPage = new Page
        {
            Id = pageId,
            SiteId = siteId,
            PageId = "page",
            Title = "Title",
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var command = new UpdatePageCommand
        {
            Id = pageId,
            SiteId = siteId,
            PageId = "page",
            Title = "Updated Title"
        };

        Page updatedPage = null!;
        _pageRepositoryMock.Setup(r => r.GetByIdAsync(pageId)).ReturnsAsync(existingPage);
        _pageRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Page>()))
            .Callback<Page>(p => updatedPage = p)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        updatedPage.Should().NotBeNull();
        updatedPage.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}

using CMS.Application.Features.Pages.Queries;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CMS.Application.Tests.Features.Pages;

public class GetPageByIdQueryHandlerTests
{
    private readonly Mock<IRepository<Page>> _pageRepositoryMock;
    private readonly GetPageByIdQueryHandler _handler;

    public GetPageByIdQueryHandlerTests()
    {
        _pageRepositoryMock = new Mock<IRepository<Page>>();
        _handler = new GetPageByIdQueryHandler(_pageRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Page_When_Found()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var page = new Page
        {
            Id = pageId,
            SiteId = siteId,
            PageId = "home",
            Title = "Home Page",
            IsDeleted = false
        };

        var query = new GetPageByIdQuery { Id = pageId, SiteId = siteId };

        _pageRepositoryMock.Setup(r => r.GetByIdAsync(pageId)).ReturnsAsync(page);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(pageId);
        result.PageId.Should().Be("home");
        result.Title.Should().Be("Home Page");
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Page_Not_Found()
    {
        // Arrange
        var query = new GetPageByIdQuery
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid()
        };

        _pageRepositoryMock.Setup(r => r.GetByIdAsync(query.Id)).ReturnsAsync((Page?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_SiteId_Mismatch()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var page = new Page
        {
            Id = pageId,
            SiteId = Guid.NewGuid(),
            PageId = "page",
            Title = "Page",
            IsDeleted = false
        };

        var query = new GetPageByIdQuery
        {
            Id = pageId,
            SiteId = Guid.NewGuid() // Different SiteId
        };

        _pageRepositoryMock.Setup(r => r.GetByIdAsync(pageId)).ReturnsAsync(page);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Page_Is_Deleted()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var page = new Page
        {
            Id = pageId,
            SiteId = siteId,
            PageId = "deleted",
            Title = "Deleted Page",
            IsDeleted = true
        };

        var query = new GetPageByIdQuery { Id = pageId, SiteId = siteId };

        _pageRepositoryMock.Setup(r => r.GetByIdAsync(pageId)).ReturnsAsync(page);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}

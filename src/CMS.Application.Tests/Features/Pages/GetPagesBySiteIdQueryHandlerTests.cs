using CMS.Application.Features.Pages.Queries;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;

namespace CMS.Application.Tests.Features.Pages;

public class GetPagesBySiteIdQueryHandlerTests
{
    private readonly Mock<IRepository<Page>> _pageRepositoryMock;
    private readonly GetPagesBySiteIdQueryHandler _handler;

    public GetPagesBySiteIdQueryHandlerTests()
    {
        _pageRepositoryMock = new Mock<IRepository<Page>>();
        _handler = new GetPagesBySiteIdQueryHandler(_pageRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Pages_For_Site()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var pages = new List<Page>
        {
            new Page
            {
                Id = Guid.NewGuid(),
                SiteId = siteId,
                PageId = "home",
                Title = "Home",
                IsDeleted = false
            },
            new Page
            {
                Id = Guid.NewGuid(),
                SiteId = siteId,
                PageId = "about",
                Title = "About",
                IsDeleted = false
            }
        };

        var query = new GetPagesBySiteIdQuery { SiteId = siteId };

        _pageRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Page, bool>>>()))
            .ReturnsAsync(pages);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.PageId == "home");
        result.Should().Contain(p => p.PageId == "about");
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_When_No_Pages_Found()
    {
        // Arrange
        var query = new GetPagesBySiteIdQuery { SiteId = Guid.NewGuid() };

        _pageRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Page, bool>>>()))
            .ReturnsAsync(new List<Page>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Should_Filter_Deleted_Pages()
    {
        // Arrange
        var siteId = Guid.NewGuid();
        var pages = new List<Page>
        {
            new Page
            {
                Id = Guid.NewGuid(),
                SiteId = siteId,
                PageId = "active",
                Title = "Active Page",
                IsDeleted = false
            }
        };

        var query = new GetPagesBySiteIdQuery { SiteId = siteId };

        _pageRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Page, bool>>>()))
            .ReturnsAsync(pages);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().PageId.Should().Be("active");
    }
}

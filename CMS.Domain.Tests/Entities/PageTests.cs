using CMS.Domain.Entities;
using FluentAssertions;

namespace CMS.Domain.Tests.Entities;

public class PageTests
{
    [Fact]
    public void Page_Should_Initialize_With_Valid_Properties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var pageId = "home";
        var title = "Home Page";

        // Act
        var page = new Page
        {
            Id = id,
            SiteId = siteId,
            PageId = pageId,
            Title = title,
            IsPublished = true
        };

        // Assert
        page.Id.Should().Be(id);
        page.SiteId.Should().Be(siteId);
        page.PageId.Should().Be(pageId);
        page.Title.Should().Be(title);
        page.IsPublished.Should().BeTrue();
        page.Contents.Should().NotBeNull();
    }

    [Fact]
    public void Page_Should_Allow_Adding_Content()
    {
        // Arrange
        var page = new Page
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
            PageId = "about",
            Title = "About Us"
        };
        var content = new PageContent
        {
            Id = Guid.NewGuid(),
            PageId = page.Id,
            ContentId = "intro",
            Content = "Welcome to our site",
            Order = 1
        };

        // Act
        page.Contents.Add(content);

        // Assert
        page.Contents.Should().HaveCount(1);
        page.Contents.First().ContentId.Should().Be("intro");
        page.Contents.First().Content.Should().Be("Welcome to our site");
        page.Contents.First().Order.Should().Be(1);
    }

    [Fact]
    public void Page_Should_Support_Multiple_Contents_In_Order()
    {
        // Arrange
        var page = new Page
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
            PageId = "services",
            Title = "Our Services"
        };

        // Act
        page.Contents.Add(new PageContent { Id = Guid.NewGuid(), PageId = page.Id, ContentId = "header", Content = "Header", Order = 1 });
        page.Contents.Add(new PageContent { Id = Guid.NewGuid(), PageId = page.Id, ContentId = "body", Content = "Body", Order = 2 });
        page.Contents.Add(new PageContent { Id = Guid.NewGuid(), PageId = page.Id, ContentId = "footer", Content = "Footer", Order = 3 });

        // Assert
        page.Contents.Should().HaveCount(3);
        page.Contents.OrderBy(c => c.Order).First().ContentId.Should().Be("header");
        page.Contents.OrderBy(c => c.Order).Last().ContentId.Should().Be("footer");
    }
}

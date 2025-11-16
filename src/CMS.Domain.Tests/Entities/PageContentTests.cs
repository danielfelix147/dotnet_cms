using CMS.Domain.Entities;
using FluentAssertions;

namespace CMS.Domain.Tests.Entities;

public class PageContentTests
{
    [Fact]
    public void PageContent_Should_Initialize_With_Valid_Properties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var pageId = Guid.NewGuid();
        var contentId = "section-1";
        var content = "<h1>Welcome</h1><p>This is the introduction.</p>";
        var order = 1;

        // Act
        var pageContent = new PageContent
        {
            Id = id,
            PageId = pageId,
            ContentId = contentId,
            Content = content,
            Order = order
        };

        // Assert
        pageContent.Id.Should().Be(id);
        pageContent.PageId.Should().Be(pageId);
        pageContent.ContentId.Should().Be(contentId);
        pageContent.Content.Should().Be(content);
        pageContent.Order.Should().Be(order);
    }

    [Fact]
    public void PageContent_Should_Support_Html_Content()
    {
        // Arrange
        var htmlContent = @"
            <div class='section'>
                <h2>About Us</h2>
                <p>We are a leading company in <strong>web development</strong>.</p>
                <ul>
                    <li>Service 1</li>
                    <li>Service 2</li>
                </ul>
            </div>";

        // Act
        var pageContent = new PageContent
        {
            Id = Guid.NewGuid(),
            PageId = Guid.NewGuid(),
            ContentId = "about-section",
            Content = htmlContent,
            Order = 2
        };

        // Assert
        pageContent.Content.Should().Contain("<h2>About Us</h2>");
        pageContent.Content.Should().Contain("<strong>web development</strong>");
        pageContent.Content.Should().Contain("<li>Service 1</li>");
    }

    [Fact]
    public void PageContent_Should_Maintain_Order_For_Sequencing()
    {
        // Arrange
        var pageId = Guid.NewGuid();
        var contents = new List<PageContent>
        {
            new() { Id = Guid.NewGuid(), PageId = pageId, ContentId = "footer", Content = "Footer", Order = 3 },
            new() { Id = Guid.NewGuid(), PageId = pageId, ContentId = "header", Content = "Header", Order = 1 },
            new() { Id = Guid.NewGuid(), PageId = pageId, ContentId = "body", Content = "Body", Order = 2 }
        };

        // Act
        var orderedContents = contents.OrderBy(c => c.Order).ToList();

        // Assert
        orderedContents[0].ContentId.Should().Be("header");
        orderedContents[1].ContentId.Should().Be("body");
        orderedContents[2].ContentId.Should().Be("footer");
    }
}

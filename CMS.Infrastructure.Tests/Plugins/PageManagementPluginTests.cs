using CMS.Domain.Entities;
using CMS.Infrastructure.Plugins;
using FluentAssertions;
using System.Text.Json;

namespace CMS.Infrastructure.Tests.Plugins;

public class PageManagementPluginTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public PageManagementPluginTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Plugin_Properties_Should_Be_Correct()
    {
        // Arrange
        var context = _fixture.CreateDbContext();
        var plugin = new PageManagementPlugin(context);

        // Assert
        plugin.SystemName.Should().Be("PageManagement");
        plugin.DisplayName.Should().Be("Page Management");
        plugin.Description.Should().Be("Manage website pages with content, images, and files");
        plugin.Version.Should().Be("1.0.0");
    }

    [Fact]
    public async Task GetContentAsync_Should_Return_Published_Pages()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var plugin = new PageManagementPlugin(context);

        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Test Site",
            Domain = $"page-plugin-{Guid.NewGuid()}.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Sites.Add(site);
        await context.SaveChangesAsync();

        var page = new Page
        {
            Id = Guid.NewGuid(),
            SiteId = site.Id,
            PageId = "home",
            Title = "Home Page",
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        // Act
        var result = await plugin.GetContentAsync(site.Id);

        // Assert
        result.Should().NotBeNull();
        var resultList = result as List<object>;
        resultList.Should().NotBeNull();
        resultList!.Should().HaveCountGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task GetContentAsync_Should_Include_Page_Images_And_Files()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var plugin = new PageManagementPlugin(context);

        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Test Site",
            Domain = $"page-media-{Guid.NewGuid()}.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Sites.Add(site);
        await context.SaveChangesAsync();

        var page = new Page
        {
            Id = Guid.NewGuid(),
            SiteId = site.Id,
            PageId = "about",
            Title = "About Us",
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Pages.Add(page);
        await context.SaveChangesAsync();

        var image = new Image
        {
            Id = Guid.NewGuid(),
            EntityId = page.Id,
            EntityType = "Page",
            ImageId = "img1",
            Location = "/images/about.jpg",
            CreatedAt = DateTime.UtcNow
        };
        context.Images.Add(image);
        await context.SaveChangesAsync();

        // Act
        var result = await plugin.GetContentAsync(site.Id);

        // Assert
        result.Should().NotBeNull();
        var json = JsonSerializer.Serialize(result);
        json.Should().Contain("img1");
        json.Should().Contain("/images/about.jpg");
    }

    [Fact]
    public async Task GenerateJsonAsync_Should_Return_Valid_Json()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var plugin = new PageManagementPlugin(context);

        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Test Site",
            Domain = $"page-json-{Guid.NewGuid()}.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Sites.Add(site);
        await context.SaveChangesAsync();

        // Act
        var result = await plugin.GenerateJsonAsync(site.Id);

        // Assert
        result.Should().NotBeNullOrEmpty();
        var action = () => JsonDocument.Parse(result);
        action.Should().NotThrow();
    }
}

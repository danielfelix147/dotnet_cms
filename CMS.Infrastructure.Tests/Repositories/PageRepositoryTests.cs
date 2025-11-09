using CMS.Domain.Entities;
using CMS.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CMS.Infrastructure.Tests.Repositories;

public class PageRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public PageRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetPagesBySiteIdAsync_Should_Return_Pages_For_Site()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var pageRepository = new Repository<Page>(context);
        var siteRepository = new SiteRepository(context);

        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Test Site",
            Domain = $"pagetest-{Guid.NewGuid()}.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await siteRepository.AddAsync(site);

        var page1 = new Page
        {
            Id = Guid.NewGuid(),
            SiteId = site.Id,
            PageId = "home",
            Title = "Home Page",
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };
        var page2 = new Page
        {
            Id = Guid.NewGuid(),
            SiteId = site.Id,
            PageId = "about",
            Title = "About Page",
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };

        await pageRepository.AddAsync(page1);
        await pageRepository.AddAsync(page2);
        await context.SaveChangesAsync();

        // Act
        var pages = await pageRepository.GetAllAsync();
        var sitePages = pages.Where(p => p.SiteId == site.Id).ToList();

        // Assert
        sitePages.Should().HaveCount(2);
        sitePages.Should().Contain(p => p.PageId == "home");
        sitePages.Should().Contain(p => p.PageId == "about");
    }

    [Fact]
    public async Task Page_Should_Support_Multiple_Contents()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var pageRepository = new Repository<Page>(context);
        var siteRepository = new SiteRepository(context);

        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Test Site",
            Domain = $"content-{Guid.NewGuid()}.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        await siteRepository.AddAsync(site);

        var page = new Page
        {
            Id = Guid.NewGuid(),
            SiteId = site.Id,
            PageId = "services",
            Title = "Services",
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };

        page.Contents.Add(new PageContent
        {
            Id = Guid.NewGuid(),
            PageId = page.Id,
            ContentId = "intro",
            Content = "Welcome to our services",
            Order = 1,
            CreatedAt = DateTime.UtcNow
        });

        page.Contents.Add(new PageContent
        {
            Id = Guid.NewGuid(),
            PageId = page.Id,
            ContentId = "details",
            Content = "Service details here",
            Order = 2,
            CreatedAt = DateTime.UtcNow
        });

        await pageRepository.AddAsync(page);
        await context.SaveChangesAsync();

        // Act
        var savedPage = await context.Pages
            .Include(p => p.Contents)
            .FirstOrDefaultAsync(p => p.Id == page.Id);

        // Assert
        savedPage.Should().NotBeNull();
        savedPage!.Contents.Should().HaveCount(2);
        savedPage.Contents.Should().Contain(c => c.ContentId == "intro");
        savedPage.Contents.Should().Contain(c => c.ContentId == "details");
    }
}

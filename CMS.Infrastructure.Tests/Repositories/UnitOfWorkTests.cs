using CMS.Domain.Entities;
using CMS.Infrastructure.Repositories;
using FluentAssertions;

namespace CMS.Infrastructure.Tests.Repositories;

public class UnitOfWorkTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public UnitOfWorkTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SaveChangesAsync_Should_Persist_Multiple_Entities()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var unitOfWork = new UnitOfWork(context);
        var siteRepository = new SiteRepository(context);
        var pageRepository = new Repository<Page>(context);

        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "UoW Test Site",
            Domain = $"uow-{Guid.NewGuid()}.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var page = new Page
        {
            Id = Guid.NewGuid(),
            SiteId = site.Id,
            PageId = "test",
            Title = "Test Page",
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };

        await siteRepository.AddAsync(site);
        await pageRepository.AddAsync(page);

        // Act
        var result = await unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().BeGreaterThan(0);

        var savedSite = await siteRepository.GetByIdAsync(site.Id);
        var savedPage = await pageRepository.GetByIdAsync(page.Id);

        savedSite.Should().NotBeNull();
        savedPage.Should().NotBeNull();
    }

    [Fact]
    public async Task Transaction_Should_Rollback_On_Error()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var siteRepository = new SiteRepository(context);

        var site1 = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Site 1",
            Domain = $"trans1-{Guid.NewGuid()}.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var site2 = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Site 2",
            Domain = site1.Domain, // Duplicate domain should violate unique constraint
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await siteRepository.AddAsync(site1);
        await context.SaveChangesAsync();

        // Act
        await siteRepository.AddAsync(site2);
        var action = async () => await context.SaveChangesAsync();

        // Assert
        await action.Should().ThrowAsync<Exception>();
    }
}

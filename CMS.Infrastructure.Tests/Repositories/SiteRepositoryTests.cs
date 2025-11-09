using CMS.Domain.Entities;
using CMS.Infrastructure.Repositories;
using FluentAssertions;

namespace CMS.Infrastructure.Tests.Repositories;

public class SiteRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public SiteRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_Should_Add_Site_To_Database()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new SiteRepository(context);
        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Test Site",
            Domain = "testsite.com",
            Description = "A test site",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await repository.AddAsync(site);
        await context.SaveChangesAsync();

        // Assert
        var savedSite = await repository.GetByIdAsync(site.Id);
        savedSite.Should().NotBeNull();
        savedSite!.Name.Should().Be("Test Site");
        savedSite.Domain.Should().Be("testsite.com");
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Sites()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new SiteRepository(context);

        var site1 = new Site { Id = Guid.NewGuid(), Name = "Site 1", Domain = "site1.com", IsActive = true, CreatedAt = DateTime.UtcNow };
        var site2 = new Site { Id = Guid.NewGuid(), Name = "Site 2", Domain = "site2.com", IsActive = true, CreatedAt = DateTime.UtcNow };

        await repository.AddAsync(site1);
        await repository.AddAsync(site2);
        await context.SaveChangesAsync();

        // Act
        var sites = await repository.GetAllAsync();

        // Assert
        sites.Should().HaveCountGreaterOrEqualTo(2);
        sites.Should().Contain(s => s.Domain == "site1.com");
        sites.Should().Contain(s => s.Domain == "site2.com");
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Site_Not_Found()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new SiteRepository(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var site = await repository.GetByIdAsync(nonExistentId);

        // Assert
        site.Should().BeNull();
    }

    [Fact]
    public async Task GetByDomainAsync_Should_Return_Site_With_Matching_Domain()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new SiteRepository(context);
        var domain = $"uniquesite-{Guid.NewGuid()}.com";
        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Unique Site",
            Domain = domain,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(site);
        await context.SaveChangesAsync();

        // Act
        var foundSite = await repository.GetByDomainAsync(domain);

        // Assert
        foundSite.Should().NotBeNull();
        foundSite!.Domain.Should().Be(domain);
        foundSite.Name.Should().Be("Unique Site");
    }

    [Fact]
    public async Task Update_Should_Modify_Site_Properties()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new SiteRepository(context);
        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Original Name",
            Domain = $"original-{Guid.NewGuid()}.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(site);
        await context.SaveChangesAsync();

        // Act
        site.Name = "Updated Name";
        site.Description = "Updated Description";
        site.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        // Assert
        var updatedSite = await repository.GetByIdAsync(site.Id);
        updatedSite.Should().NotBeNull();
        updatedSite!.Name.Should().Be("Updated Name");
        updatedSite.Description.Should().Be("Updated Description");
        updatedSite.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_Should_Remove_Site_From_Database()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repository = new SiteRepository(context);
        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Site To Delete",
            Domain = $"delete-{Guid.NewGuid()}.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(site);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(site);
        await context.SaveChangesAsync();

        // Assert
        var deletedSite = await repository.GetByIdAsync(site.Id);
        deletedSite.Should().BeNull();
    }
}

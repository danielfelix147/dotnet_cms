using CMS.Domain.Entities;
using FluentAssertions;

namespace CMS.Domain.Tests.Entities;

public class SiteTests
{
    [Fact]
    public void Site_Should_Initialize_With_Valid_Properties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Test Site";
        var domain = "testsite.com";
        var description = "A test site";

        // Act
        var site = new Site
        {
            Id = id,
            Name = name,
            Domain = domain,
            Description = description,
            IsActive = true
        };

        // Assert
        site.Id.Should().Be(id);
        site.Name.Should().Be(name);
        site.Domain.Should().Be(domain);
        site.Description.Should().Be(description);
        site.IsActive.Should().BeTrue();
        site.SiteUsers.Should().NotBeNull();
        site.SitePlugins.Should().NotBeNull();
    }

    [Fact]
    public void Site_Should_Allow_Adding_Users()
    {
        // Arrange
        var site = new Site { Id = Guid.NewGuid(), Name = "Test Site", Domain = "test.com" };
        var userId = Guid.NewGuid().ToString();
        var siteUser = new SiteUser
        {
            Id = Guid.NewGuid(),
            SiteId = site.Id,
            UserId = userId,
            Site = site,
            Role = "Editor"
        };

        // Act
        site.SiteUsers.Add(siteUser);

        // Assert
        site.SiteUsers.Should().HaveCount(1);
        site.SiteUsers.First().UserId.Should().Be(userId);
        site.SiteUsers.First().Role.Should().Be("Editor");
    }

    [Fact]
    public void Site_Should_Allow_Adding_Plugins()
    {
        // Arrange
        var site = new Site { Id = Guid.NewGuid(), Name = "Test Site", Domain = "test.com" };
        var plugin = new Plugin { Id = Guid.NewGuid(), Name = "Page Plugin", SystemName = "PageManagement", IsActive = true };
        var sitePlugin = new SitePlugin
        {
            Id = Guid.NewGuid(),
            SiteId = site.Id,
            PluginId = plugin.Id,
            Site = site,
            Plugin = plugin,
            IsEnabled = true
        };

        // Act
        site.SitePlugins.Add(sitePlugin);

        // Assert
        site.SitePlugins.Should().HaveCount(1);
        site.SitePlugins.First().PluginId.Should().Be(plugin.Id);
        site.SitePlugins.First().IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void Site_Should_Inherit_BaseEntity_Properties()
    {
        // Arrange & Act
        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Test Site",
            Domain = "test.com",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestUser",
            IsDeleted = false
        };

        // Assert
        site.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        site.CreatedBy.Should().Be("TestUser");
        site.IsDeleted.Should().BeFalse();
        site.UpdatedAt.Should().BeNull();
        site.UpdatedBy.Should().BeNull();
    }
}

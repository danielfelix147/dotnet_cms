using CMS.Domain.Entities;
using FluentAssertions;

namespace CMS.Domain.Tests.Entities;

public class PluginTests
{
    [Fact]
    public void Plugin_Should_Initialize_With_Valid_Properties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Page Management";
        var systemName = "PageManagement";
        var description = "Manage pages and content";
        var isActive = true;

        // Act
        var plugin = new Plugin
        {
            Id = id,
            Name = name,
            SystemName = systemName,
            Description = description,
            IsActive = isActive
        };

        // Assert
        plugin.Id.Should().Be(id);
        plugin.Name.Should().Be(name);
        plugin.SystemName.Should().Be(systemName);
        plugin.Description.Should().Be(description);
        plugin.IsActive.Should().BeTrue();
        plugin.SitePlugins.Should().NotBeNull();
    }

    [Fact]
    public void Plugin_Should_Allow_Adding_Site_Associations()
    {
        // Arrange
        var plugin = new Plugin
        {
            Id = Guid.NewGuid(),
            Name = "Product Management",
            SystemName = "ProductManagement",
            IsActive = true
        };
        var sitePlugin = new SitePlugin
        {
            Id = Guid.NewGuid(),
            PluginId = plugin.Id,
            SiteId = Guid.NewGuid(),
            IsEnabled = true
        };

        // Act
        plugin.SitePlugins.Add(sitePlugin);

        // Assert
        plugin.SitePlugins.Should().HaveCount(1);
        plugin.SitePlugins.First().PluginId.Should().Be(plugin.Id);
        plugin.SitePlugins.First().IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void Plugin_Should_Support_Multiple_Site_Associations()
    {
        // Arrange
        var plugin = new Plugin
        {
            Id = Guid.NewGuid(),
            Name = "Travel Management",
            SystemName = "TravelManagement",
            IsActive = true
        };
        var site1Id = Guid.NewGuid();
        var site2Id = Guid.NewGuid();
        var site3Id = Guid.NewGuid();

        // Act
        plugin.SitePlugins.Add(new SitePlugin { Id = Guid.NewGuid(), PluginId = plugin.Id, SiteId = site1Id, IsEnabled = true });
        plugin.SitePlugins.Add(new SitePlugin { Id = Guid.NewGuid(), PluginId = plugin.Id, SiteId = site2Id, IsEnabled = false });
        plugin.SitePlugins.Add(new SitePlugin { Id = Guid.NewGuid(), PluginId = plugin.Id, SiteId = site3Id, IsEnabled = true });

        // Assert
        plugin.SitePlugins.Should().HaveCount(3);
        plugin.SitePlugins.Count(sp => sp.IsEnabled).Should().Be(2);
        plugin.SitePlugins.Select(sp => sp.SiteId).Should().Contain(new[] { site1Id, site2Id, site3Id });
    }

    [Fact]
    public void Plugin_Should_Allow_Nullable_Description()
    {
        // Arrange & Act
        var plugin = new Plugin
        {
            Id = Guid.NewGuid(),
            Name = "Simple Plugin",
            SystemName = "SimplePlugin",
            IsActive = true
        };

        // Assert
        plugin.Description.Should().BeNull();
    }
}

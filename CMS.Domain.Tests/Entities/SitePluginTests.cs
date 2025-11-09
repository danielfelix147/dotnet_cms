using CMS.Domain.Entities;
using FluentAssertions;

namespace CMS.Domain.Tests.Entities;

public class SitePluginTests
{
    [Fact]
    public void SitePlugin_Should_Initialize_With_Valid_Properties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var siteId = Guid.NewGuid();
        var pluginId = Guid.NewGuid();
        var isEnabled = true;
        var configuration = "{\"theme\":\"dark\",\"showHeader\":true}";

        // Act
        var sitePlugin = new SitePlugin
        {
            Id = id,
            SiteId = siteId,
            PluginId = pluginId,
            IsEnabled = isEnabled,
            Configuration = configuration
        };

        // Assert
        sitePlugin.Id.Should().Be(id);
        sitePlugin.SiteId.Should().Be(siteId);
        sitePlugin.PluginId.Should().Be(pluginId);
        sitePlugin.IsEnabled.Should().BeTrue();
        sitePlugin.Configuration.Should().Be(configuration);
    }

    [Fact]
    public void SitePlugin_Should_Allow_Json_Configuration()
    {
        // Arrange
        var jsonConfig = @"{
            ""maxItems"": 10,
            ""sortOrder"": ""asc"",
            ""filters"": [""category"", ""date""],
            ""settings"": {
                ""showImages"": true,
                ""showDescriptions"": false
            }
        }";

        // Act
        var sitePlugin = new SitePlugin
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
            PluginId = Guid.NewGuid(),
            IsEnabled = true,
            Configuration = jsonConfig
        };

        // Assert
        sitePlugin.Configuration.Should().Contain("maxItems");
        sitePlugin.Configuration.Should().Contain("showImages");
        sitePlugin.Configuration.Should().Contain("filters");
    }

    [Fact]
    public void SitePlugin_Should_Allow_Nullable_Configuration()
    {
        // Arrange & Act
        var sitePlugin = new SitePlugin
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
            PluginId = Guid.NewGuid(),
            IsEnabled = false
        };

        // Assert
        sitePlugin.Configuration.Should().BeNull();
    }

    [Fact]
    public void SitePlugin_Should_Track_Enabled_State()
    {
        // Arrange
        var sitePlugin = new SitePlugin
        {
            Id = Guid.NewGuid(),
            SiteId = Guid.NewGuid(),
            PluginId = Guid.NewGuid(),
            IsEnabled = true
        };

        // Act
        sitePlugin.IsEnabled = false;

        // Assert
        sitePlugin.IsEnabled.Should().BeFalse();
    }
}

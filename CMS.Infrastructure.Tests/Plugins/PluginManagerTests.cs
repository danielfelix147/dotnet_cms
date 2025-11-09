using CMS.Domain.Entities;
using CMS.Domain.Plugins;
using CMS.Infrastructure.Data;
using CMS.Infrastructure.Plugins;
using FluentAssertions;
using System.Text.Json;

namespace CMS.Infrastructure.Tests.Plugins;

public class PluginManagerTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public PluginManagerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void GetAllPlugins_Should_Return_All_Registered_Plugins()
    {
        // Arrange
        var context = _fixture.CreateDbContext();
        var pagePlugin = new PageManagementPlugin(context);
        var productPlugin = new ProductManagementPlugin(context);
        var travelPlugin = new TravelManagementPlugin(context);
        IEnumerable<IPlugin> plugins = new IPlugin[] { pagePlugin, productPlugin, travelPlugin };
        
        var pluginManager = new PluginManager(plugins, context);

        // Act
        var result = pluginManager.GetAllPlugins();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(p => p.SystemName == "PageManagement");
        result.Should().Contain(p => p.SystemName == "ProductManagement");
        result.Should().Contain(p => p.SystemName == "TravelManagement");
    }

    [Fact]
    public void GetPlugin_WithValidSystemName_Should_Return_Plugin()
    {
        // Arrange
        var context = _fixture.CreateDbContext();
        var pagePlugin = new PageManagementPlugin(context);
        IEnumerable<IPlugin> plugins = new IPlugin[] { pagePlugin };
        
        var pluginManager = new PluginManager(plugins, context);

        // Act
        var result = pluginManager.GetPlugin("PageManagement");

        // Assert
        result.Should().NotBeNull();
        result!.SystemName.Should().Be("PageManagement");
        result.DisplayName.Should().Be("Page Management");
    }

    [Fact]
    public void GetPlugin_WithInvalidSystemName_Should_Return_Null()
    {
        // Arrange
        var context = _fixture.CreateDbContext();
        IEnumerable<IPlugin> plugins = Array.Empty<IPlugin>();
        
        var pluginManager = new PluginManager(plugins, context);

        // Act
        var result = pluginManager.GetPlugin("NonExistentPlugin");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEnabledPluginsForSiteAsync_Should_Return_Only_Enabled_Plugins()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        
        // Create plugins with unique system names
        var pluginSystemName = $"PageManagement-{Guid.NewGuid()}";
        var plugin1 = new Domain.Entities.Plugin
        {
            Id = Guid.NewGuid(),
            Name = "Page Management",
            SystemName = pluginSystemName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var plugin2SystemName = $"ProductManagement-{Guid.NewGuid()}";
        var plugin2 = new Domain.Entities.Plugin
        {
            Id = Guid.NewGuid(),
            Name = "Product Management",
            SystemName = plugin2SystemName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Plugins.AddRange(plugin1, plugin2);
        await context.SaveChangesAsync();

        // Create site
        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Test Site",
            Domain = $"plugin-test-{Guid.NewGuid()}.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Sites.Add(site);
        await context.SaveChangesAsync();

        // Enable only PageManagement plugin
        var sitePlugin = new SitePlugin
        {
            Id = Guid.NewGuid(),
            SiteId = site.Id,
            PluginId = plugin1.Id,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };
        context.SitePlugins.Add(sitePlugin);
        await context.SaveChangesAsync();

        // Create mock plugins that match the system name
        var pagePlugin = new PageManagementPlugin(context);
        var productPlugin = new ProductManagementPlugin(context);
        
        // Override SystemName to match database
        var plugins = new List<IPlugin>
        {
            new TestPlugin(pluginSystemName, "Page Management"),
            new TestPlugin(plugin2SystemName, "Product Management")
        };
        
        var pluginManager = new PluginManager(plugins, context);

        // Act
        var result = await pluginManager.GetEnabledPluginsForSiteAsync(site.Id);

        // Assert
        result.Should().HaveCount(1);
        result.First().SystemName.Should().Be(pluginSystemName);
    }

    // Helper test plugin class
    private class TestPlugin : BasePlugin
    {
        private readonly string _systemName;
        private readonly string _displayName;

        public TestPlugin(string systemName, string displayName)
        {
            _systemName = systemName;
            _displayName = displayName;
        }

        public override string SystemName => _systemName;
        public override string DisplayName => _displayName;
        public override string Description => "Test Plugin";
        public override string Version => "1.0.0";

        public override Task<object> GetContentAsync(Guid siteId)
        {
            return Task.FromResult<object>(new { });
        }

        public override Task<string> GenerateJsonAsync(Guid siteId)
        {
            return Task.FromResult("{}");
        }
    }

    [Fact]
    public async Task GenerateSiteJsonAsync_Should_Generate_Json_For_Enabled_Plugins()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();

        // Create plugin with unique system name
        var pluginSystemName = $"PageManagement-{Guid.NewGuid()}";
        var plugin = new Domain.Entities.Plugin
        {
            Id = Guid.NewGuid(),
            Name = "Page Management",
            SystemName = pluginSystemName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Plugins.Add(plugin);
        await context.SaveChangesAsync();

        // Create site
        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = "Test Site",
            Domain = $"json-test-{Guid.NewGuid()}.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Sites.Add(site);
        await context.SaveChangesAsync();

        // Enable plugin for site
        var sitePlugin = new SitePlugin
        {
            Id = Guid.NewGuid(),
            SiteId = site.Id,
            PluginId = plugin.Id,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };
        context.SitePlugins.Add(sitePlugin);
        await context.SaveChangesAsync();

        var plugins = new List<IPlugin>
        {
            new TestPlugin(pluginSystemName, "Page Management")
        };
        
        var pluginManager = new PluginManager(plugins, context);

        // Act
        var result = await pluginManager.GenerateSiteJsonAsync(site.Id);

        // Assert
        result.Should().NotBeNullOrEmpty();
        var jsonDoc = JsonDocument.Parse(result);
        jsonDoc.RootElement.TryGetProperty(pluginSystemName, out _).Should().BeTrue();
    }
}

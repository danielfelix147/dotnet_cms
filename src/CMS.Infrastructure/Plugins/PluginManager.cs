using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using CMS.Domain.Plugins;
using CMS.Infrastructure.Data;

namespace CMS.Infrastructure.Plugins;

public class PluginManager : IPluginManager
{
    private readonly IEnumerable<IPlugin> _plugins;
    private readonly CMSDbContext _context;

    public PluginManager(IEnumerable<IPlugin> plugins, CMSDbContext context)
    {
        _plugins = plugins;
        _context = context;
    }

    public IEnumerable<IPlugin> GetAllPlugins()
    {
        return _plugins;
    }

    public IPlugin? GetPlugin(string systemName)
    {
        return _plugins.FirstOrDefault(p => p.SystemName == systemName);
    }

    public async Task<IEnumerable<IPlugin>> GetEnabledPluginsForSiteAsync(Guid siteId)
    {
        var enabledPluginIds = await _context.SitePlugins
            .Where(sp => sp.SiteId == siteId && sp.IsEnabled)
            .Include(sp => sp.Plugin)
            .Select(sp => sp.Plugin.SystemName)
            .ToListAsync();

        return _plugins.Where(p => enabledPluginIds.Contains(p.SystemName));
    }

    public async Task<string> GenerateSiteJsonAsync(Guid siteId)
    {
        var enabledPlugins = await GetEnabledPluginsForSiteAsync(siteId);
        var siteData = new Dictionary<string, object>();

        foreach (var plugin in enabledPlugins.OfType<IContentPlugin>())
        {
            var content = await plugin.GetContentAsync(siteId);
            siteData[plugin.SystemName] = content;
        }

        return JsonSerializer.Serialize(siteData, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}

namespace CMS.Domain.Plugins;

public interface IPluginManager
{
    IEnumerable<IPlugin> GetAllPlugins();
    IPlugin? GetPlugin(string systemName);
    Task<IEnumerable<IPlugin>> GetEnabledPluginsForSiteAsync(Guid siteId);
    Task<string> GenerateSiteJsonAsync(Guid siteId);
}

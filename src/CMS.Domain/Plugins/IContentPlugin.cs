namespace CMS.Domain.Plugins;

public interface IContentPlugin : IPlugin
{
    Task<string> GenerateJsonAsync(Guid siteId);
}

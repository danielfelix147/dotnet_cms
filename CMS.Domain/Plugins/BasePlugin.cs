namespace CMS.Domain.Plugins;

public abstract class BasePlugin : IContentPlugin
{
    public abstract string SystemName { get; }
    public abstract string DisplayName { get; }
    public abstract string Description { get; }
    public abstract string Version { get; }

    public abstract Task<object> GetContentAsync(Guid siteId);
    public abstract Task<string> GenerateJsonAsync(Guid siteId);

    public virtual Task ValidateConfigurationAsync(string? configuration)
    {
        // Default implementation - can be overridden by specific plugins
        return Task.CompletedTask;
    }
}

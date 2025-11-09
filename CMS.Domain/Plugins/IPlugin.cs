namespace CMS.Domain.Plugins;

public interface IPlugin
{
    string SystemName { get; }
    string DisplayName { get; }
    string Description { get; }
    string Version { get; }
    Task<object> GetContentAsync(Guid siteId);
    Task ValidateConfigurationAsync(string? configuration);
}

using CMS.Domain.Entities;

namespace CMS.Domain.Interfaces;

public interface ISiteRepository : IRepository<Site>
{
    Task<Site?> GetByDomainAsync(string domain);
    Task<IEnumerable<Site>> GetSitesByUserIdAsync(Guid userId);
    Task<Site?> GetSiteWithPluginsAsync(Guid siteId);
}

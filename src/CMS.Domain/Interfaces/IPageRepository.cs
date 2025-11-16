using CMS.Domain.Entities;

namespace CMS.Domain.Interfaces;

public interface IPageRepository : IRepository<Page>
{
    Task<Page?> GetPageWithContentsAsync(Guid pageId);
    Task<IEnumerable<Page>> GetPagesBySiteIdAsync(Guid siteId);
}

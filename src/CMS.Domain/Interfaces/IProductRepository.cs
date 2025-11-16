using CMS.Domain.Entities;

namespace CMS.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetProductsBySiteIdAsync(Guid siteId);
}

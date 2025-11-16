using Microsoft.EntityFrameworkCore;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using CMS.Infrastructure.Data;

namespace CMS.Infrastructure.Repositories;

public class SiteRepository : Repository<Site>, ISiteRepository
{
    public SiteRepository(CMSDbContext context) : base(context)
    {
    }

    public override async Task<Site?> GetByIdAsync(Guid id)
    {
        // Use FirstOrDefaultAsync instead of FindAsync to respect query filters (soft delete)
        return await _dbSet.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Site?> GetByDomainAsync(string domain)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.Domain == domain);
    }

    public async Task<IEnumerable<Site>> GetSitesByUserIdAsync(Guid userId)
    {
        var userIdString = userId.ToString();
        return await _context.SiteUsers
            .Where(su => su.UserId == userIdString)
            .Include(su => su.Site)
            .Select(su => su.Site)
            .ToListAsync();
    }

    public async Task<Site?> GetSiteWithPluginsAsync(Guid siteId)
    {
        return await _dbSet
            .Include(s => s.SitePlugins)
            .ThenInclude(sp => sp.Plugin)
            .FirstOrDefaultAsync(s => s.Id == siteId);
    }
}

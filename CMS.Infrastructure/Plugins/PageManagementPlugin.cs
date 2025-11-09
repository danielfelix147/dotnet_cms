using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using CMS.Domain.Plugins;
using CMS.Infrastructure.Data;

namespace CMS.Infrastructure.Plugins;

public class PageManagementPlugin : BasePlugin
{
    private readonly CMSDbContext _context;

    public PageManagementPlugin(CMSDbContext context)
    {
        _context = context;
    }

    public override string SystemName => "PageManagement";
    public override string DisplayName => "Page Management";
    public override string Description => "Manage website pages with content, images, and files";
    public override string Version => "1.0.0";

    public override async Task<object> GetContentAsync(Guid siteId)
    {
        var pages = await _context.Pages
            .Where(p => p.SiteId == siteId && p.IsPublished)
            .Include(p => p.Contents)
            .ToListAsync();

        var result = new List<object>();

        foreach (var page in pages)
        {
            var images = await _context.Images
                .Where(i => i.EntityId == page.Id && i.EntityType == "Page")
                .ToListAsync();

            var files = await _context.Files
                .Where(f => f.EntityId == page.Id && f.EntityType == "Page")
                .ToListAsync();

            result.Add(new
            {
                PageId = page.PageId,
                Title = page.Title,
                Description = page.Description,
                Images = images.Select(i => new { i.ImageId, i.Location }),
                Files = files.Select(f => new { f.FileId, f.Location }),
                Contents = page.Contents.OrderBy(c => c.Order).Select(c => new { c.ContentId, c.Content })
            });
        }

        return result;
    }

    public override async Task<string> GenerateJsonAsync(Guid siteId)
    {
        var content = await GetContentAsync(siteId);
        return JsonSerializer.Serialize(content, new JsonSerializerOptions { WriteIndented = true });
    }
}

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using CMS.Domain.Plugins;
using CMS.Infrastructure.Data;

namespace CMS.Infrastructure.Plugins;

public class ProductManagementPlugin : BasePlugin
{
    private readonly CMSDbContext _context;

    public ProductManagementPlugin(CMSDbContext context)
    {
        _context = context;
    }

    public override string SystemName => "ProductManagement";
    public override string DisplayName => "Product Management";
    public override string Description => "Manage products with images and files";
    public override string Version => "1.0.0";

    public override async Task<object> GetContentAsync(Guid siteId)
    {
        var products = await _context.Products
            .Where(p => p.SiteId == siteId && p.IsPublished)
            .ToListAsync();

        var result = new List<object>();

        foreach (var product in products)
        {
            var images = await _context.Images
                .Where(i => i.EntityId == product.Id && i.EntityType == "Product")
                .ToListAsync();

            var files = await _context.Files
                .Where(f => f.EntityId == product.Id && f.EntityType == "Product")
                .ToListAsync();

            result.Add(new
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Images = images.Select(i => new { i.ImageId, i.Location }),
                Files = files.Select(f => new { f.FileId, f.Location })
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

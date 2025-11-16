using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using CMS.Domain.Plugins;
using CMS.Infrastructure.Data;

namespace CMS.Infrastructure.Plugins;

public class TravelManagementPlugin : BasePlugin
{
    private readonly CMSDbContext _context;

    public TravelManagementPlugin(CMSDbContext context)
    {
        _context = context;
    }

    public override string SystemName => "TravelManagement";
    public override string DisplayName => "Travel Management";
    public override string Description => "Manage destinations and tours";
    public override string Version => "1.0.0";

    public override async Task<object> GetContentAsync(Guid siteId)
    {
        var destinations = await _context.Destinations
            .Where(d => d.SiteId == siteId && d.IsPublished)
            .Include(d => d.Tours)
            .ToListAsync();

        var result = new List<object>();

        foreach (var destination in destinations)
        {
            var destinationImages = await _context.Images
                .Where(i => i.EntityId == destination.Id && i.EntityType == "Destination")
                .ToListAsync();

            var destinationFiles = await _context.Files
                .Where(f => f.EntityId == destination.Id && f.EntityType == "Destination")
                .ToListAsync();

            var tours = new List<object>();
            foreach (var tour in destination.Tours.Where(t => t.IsPublished))
            {
                var tourImages = await _context.Images
                    .Where(i => i.EntityId == tour.Id && i.EntityType == "Tour")
                    .ToListAsync();

                var tourFiles = await _context.Files
                    .Where(f => f.EntityId == tour.Id && f.EntityType == "Tour")
                    .ToListAsync();

                tours.Add(new
                {
                    TourId = tour.TourId,
                    Name = tour.Name,
                    Description = tour.Description,
                    Price = tour.Price,
                    Images = tourImages.Select(i => new { i.ImageId, i.Location }),
                    Files = tourFiles.Select(f => new { f.FileId, f.Location })
                });
            }

            result.Add(new
            {
                DestinationId = destination.DestinationId,
                Destination = destination.Name,
                Description = destination.Description,
                Tours = tours,
                Images = destinationImages.Select(i => new { i.ImageId, i.Location }),
                Files = destinationFiles.Select(f => new { f.FileId, f.Location })
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

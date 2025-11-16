using MediatR;
using CMS.Application.DTOs;
using CMS.Domain.Interfaces;
using CMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CMS.Application.Features.Content.Queries;

public class ExportSiteContentQueryHandler : IRequestHandler<ExportSiteContentQuery, SiteExportDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISiteRepository _siteRepository;

    public ExportSiteContentQueryHandler(IUnitOfWork unitOfWork, ISiteRepository siteRepository)
    {
        _unitOfWork = unitOfWork;
        _siteRepository = siteRepository;
    }

    public async Task<SiteExportDto> Handle(ExportSiteContentQuery request, CancellationToken cancellationToken)
    {
        var site = await _siteRepository.GetByIdAsync(request.SiteId);
        if (site == null)
            throw new KeyNotFoundException($"Site with ID {request.SiteId} not found");

        var export = new SiteExportDto
        {
            SiteId = site.Id,
            Name = site.Name,
            Domain = site.Domain,
            ExportedAt = DateTime.UtcNow
        };

        // Export Pages
        var pages = await _unitOfWork.Repository<Page>()
            .FindAsync(p => p.SiteId == request.SiteId && !p.IsDeleted && p.IsPublished);

        foreach (var page in pages)
        {
            var contents = await _unitOfWork.Repository<PageContent>()
                .FindAsync(pc => pc.PageId == page.Id);

            export.Pages.Add(new PageExportDto
            {
                Id = page.Id,
                Title = page.Title,
                Slug = page.PageId, // Using PageId as slug
                IsPublished = page.IsPublished,
                Contents = contents.Select(c => new PageContentExportDto
                {
                    Key = c.ContentId,
                    Value = c.Content,
                    ContentType = "HTML" // Default content type
                }).ToList()
            });
        }

        // Export Products
        var products = await _unitOfWork.Repository<Product>()
            .FindAsync(p => p.SiteId == request.SiteId && !p.IsDeleted);

        foreach (var product in products)
        {
            export.Products.Add(new ProductExportDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description ?? string.Empty,
                Price = product.Price,
                ImageUrl = null // Products don't have direct ImageUrl property
            });
        }

        // Export Destinations with Tours
        var destinations = await _unitOfWork.Repository<Destination>()
            .FindAsync(d => d.SiteId == request.SiteId && !d.IsDeleted && d.IsPublished);

        foreach (var destination in destinations)
        {
            var tours = await _unitOfWork.Repository<Tour>()
                .FindAsync(t => t.DestinationId == destination.Id && !t.IsDeleted);

            export.Destinations.Add(new DestinationExportDto
            {
                Id = destination.Id,
                Name = destination.Name,
                Description = destination.Description ?? string.Empty,
                Location = destination.DestinationId, // Using DestinationId as location
                IsPublished = destination.IsPublished,
                Tours = tours.Select(t => new TourExportDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description ?? string.Empty,
                    Price = t.Price,
                    Duration = 0 // Tour doesn't have Duration property
                }).ToList()
            });
        }

        // Export Media
        var images = await _unitOfWork.Repository<Image>()
            .FindAsync(i => i.EntityId == request.SiteId && i.EntityType == "Site");

        foreach (var image in images)
        {
            export.Media.Add(new MediaDto
            {
                Id = image.Id,
                SiteId = request.SiteId,
                FileName = image.Title ?? image.ImageId,
                FilePath = image.Location,
                FileUrl = image.Location,
                ContentType = image.MimeType ?? "image/*",
                FileSize = image.FileSize,
                FileType = "Image",
                UploadedAt = image.CreatedAt
            });
        }

        var files = await _unitOfWork.Repository<CMS.Domain.Entities.File>()
            .FindAsync(f => f.EntityId == request.SiteId && f.EntityType == "Site");

        foreach (var file in files)
        {
            export.Media.Add(new MediaDto
            {
                Id = file.Id,
                SiteId = request.SiteId,
                FileName = file.Title ?? file.FileId,
                FilePath = file.Location,
                FileUrl = file.Location,
                ContentType = file.MimeType ?? "application/octet-stream",
                FileSize = file.FileSize,
                FileType = "File",
                UploadedAt = file.CreatedAt
            });
        }

        return export;
    }
}

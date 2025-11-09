using MediatR;
using CMS.Application.DTOs;
using CMS.Domain.Interfaces;
using CMS.Domain.Entities;

namespace CMS.Application.Features.Media.Queries;

public class GetMediaBySiteIdQueryHandler : IRequestHandler<GetMediaBySiteIdQuery, IEnumerable<MediaDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMediaBySiteIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<MediaDto>> Handle(GetMediaBySiteIdQuery request, CancellationToken cancellationToken)
    {
        var mediaList = new List<MediaDto>();

        // Get all images for the site
        var images = await _unitOfWork.Repository<Image>().GetAllAsync();
        var siteImages = images.Where(i => i.EntityId == request.SiteId && i.EntityType == "Site");

        foreach (var image in siteImages)
        {
            mediaList.Add(new MediaDto
            {
                Id = image.Id,
                SiteId = request.SiteId,
                FileName = image.Title ?? image.ImageId,
                FilePath = image.Location,
                FileUrl = image.Location,
                ContentType = image.MimeType ?? "image/*",
                FileSize = image.FileSize,
                FileType = "Image",
                UploadedAt = image.CreatedAt,
                UploadedBy = image.CreatedBy ?? "System"
            });
        }

        // Get all files for the site
        var files = await _unitOfWork.Repository<CMS.Domain.Entities.File>().GetAllAsync();
        var siteFiles = files.Where(f => f.EntityId == request.SiteId && f.EntityType == "Site");

        foreach (var file in siteFiles)
        {
            mediaList.Add(new MediaDto
            {
                Id = file.Id,
                SiteId = request.SiteId,
                FileName = file.Title ?? file.FileId,
                FilePath = file.Location,
                FileUrl = file.Location,
                ContentType = file.MimeType ?? "application/octet-stream",
                FileSize = file.FileSize,
                FileType = "File",
                UploadedAt = file.CreatedAt,
                UploadedBy = file.CreatedBy ?? "System"
            });
        }

        return mediaList.OrderByDescending(m => m.UploadedAt);
    }
}

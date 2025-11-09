using MediatR;
using CMS.Domain.Interfaces;
using CMS.Domain.Entities;

namespace CMS.Application.Features.Media.Commands;

public class UploadMediaCommandHandler : IRequestHandler<UploadMediaCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _uploadPath;

    public UploadMediaCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        
        // Ensure upload directory exists
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<Guid> Handle(UploadMediaCommand request, CancellationToken cancellationToken)
    {
        // Determine file type
        var fileType = GetFileType(request.ContentType);
        
        // Generate unique filename
        var fileExtension = Path.GetExtension(request.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(_uploadPath, uniqueFileName);
        
        // Save file to disk
        await System.IO.File.WriteAllBytesAsync(filePath, request.FileData, cancellationToken);
        
        // Create appropriate entity based on file type
        if (fileType == "Image")
        {
            var image = new Image
            {
                Id = Guid.NewGuid(),
                ImageId = uniqueFileName,
                Location = $"/uploads/{uniqueFileName}",
                AltText = request.Description,
                Title = request.FileName,
                FileSize = request.FileSize,
                MimeType = request.ContentType,
                EntityId = request.SiteId,
                EntityType = "Site",
                CreatedAt = DateTime.UtcNow
            };
            
            await _unitOfWork.Repository<Image>().AddAsync(image);
            await _unitOfWork.CommitAsync();
            
            return image.Id;
        }
        else
        {
            var file = new CMS.Domain.Entities.File
            {
                Id = Guid.NewGuid(),
                FileId = uniqueFileName,
                Location = $"/uploads/{uniqueFileName}",
                Title = request.FileName,
                FileSize = request.FileSize,
                MimeType = request.ContentType,
                EntityId = request.SiteId,
                EntityType = "Site",
                CreatedAt = DateTime.UtcNow
            };
            
            await _unitOfWork.Repository<CMS.Domain.Entities.File>().AddAsync(file);
            await _unitOfWork.CommitAsync();
            
            return file.Id;
        }
    }

    private string GetFileType(string contentType)
    {
        if (contentType.StartsWith("image/"))
            return "Image";
        else if (contentType.StartsWith("video/"))
            return "Video";
        else if (contentType.Contains("pdf") || contentType.Contains("document"))
            return "Document";
        else
            return "File";
    }
}

using MediatR;
using CMS.Domain.Interfaces;
using CMS.Domain.Entities;

namespace CMS.Application.Features.Media.Commands;

public class DeleteMediaCommandHandler : IRequestHandler<DeleteMediaCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _uploadPath;

    public DeleteMediaCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
    }

    public async Task<bool> Handle(DeleteMediaCommand request, CancellationToken cancellationToken)
    {
        if (request.MediaType == "Image")
        {
            var image = await _unitOfWork.Repository<Image>().GetByIdAsync(request.Id);
            if (image == null)
                return false;

            // Delete physical file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.Location.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            await _unitOfWork.Repository<Image>().DeleteAsync(image);
        }
        else
        {
            var file = await _unitOfWork.Repository<CMS.Domain.Entities.File>().GetByIdAsync(request.Id);
            if (file == null)
                return false;

            // Delete physical file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.Location.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            await _unitOfWork.Repository<CMS.Domain.Entities.File>().DeleteAsync(file);
        }

        await _unitOfWork.CommitAsync();
        return true;
    }
}

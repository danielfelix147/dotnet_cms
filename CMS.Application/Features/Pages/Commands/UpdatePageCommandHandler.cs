using MediatR;
using CMS.Application.Mappings;
using CMS.Application.DTOs;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.Pages.Commands;

public class UpdatePageCommandHandler : IRequestHandler<UpdatePageCommand, PageDto?>
{
    private readonly IRepository<Page> _pageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePageCommandHandler(
        IRepository<Page> pageRepository, 
        IUnitOfWork unitOfWork)
    {
        _pageRepository = pageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PageDto?> Handle(UpdatePageCommand request, CancellationToken cancellationToken)
    {
        var page = await _pageRepository.GetByIdAsync(request.Id);
        
        if (page == null || page.SiteId != request.SiteId)
            return null;

        page.PageId = request.PageId;
        page.Title = request.Title;
        page.Description = request.Description;
        page.IsPublished = request.IsPublished;
        page.UpdatedAt = DateTime.UtcNow;

        await _pageRepository.UpdateAsync(page);
        await _unitOfWork.SaveChangesAsync();

        return page.ToDto();
    }
}

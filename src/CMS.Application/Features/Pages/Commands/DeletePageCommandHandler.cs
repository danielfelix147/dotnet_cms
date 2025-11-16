using MediatR;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.Pages.Commands;

public class DeletePageCommandHandler : IRequestHandler<DeletePageCommand, bool>
{
    private readonly IRepository<Page> _pageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePageCommandHandler(IRepository<Page> pageRepository, IUnitOfWork unitOfWork)
    {
        _pageRepository = pageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeletePageCommand request, CancellationToken cancellationToken)
    {
        var page = await _pageRepository.GetByIdAsync(request.Id);
        
        if (page == null || page.SiteId != request.SiteId)
            return false;

        page.IsDeleted = true;
        page.UpdatedAt = DateTime.UtcNow;

        await _pageRepository.UpdateAsync(page);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

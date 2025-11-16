using MediatR;
using CMS.Application.DTOs;
using CMS.Application.Mappings;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.Pages.Commands;

public class CreatePageCommandHandler : IRequestHandler<CreatePageCommand, PageDto>
{
    private readonly IRepository<Page> _pageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePageCommandHandler(
        IRepository<Page> pageRepository, 
        IUnitOfWork unitOfWork)
    {
        _pageRepository = pageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PageDto> Handle(CreatePageCommand request, CancellationToken cancellationToken)
    {
        var page = new Page
        {
            Id = Guid.NewGuid(),
            SiteId = request.SiteId,
            PageId = request.PageId,
            Title = request.Title,
            Description = request.Description,
            IsPublished = request.IsPublished,
            CreatedAt = DateTime.UtcNow
        };

        await _pageRepository.AddAsync(page);
        await _unitOfWork.SaveChangesAsync();

        return page.ToDto();
    }
}

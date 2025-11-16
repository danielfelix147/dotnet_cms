using MediatR;
using CMS.Application.Mappings;
using CMS.Application.DTOs;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.Pages.Queries;

public class GetPageByIdQueryHandler : IRequestHandler<GetPageByIdQuery, PageDto?>
{
    private readonly IRepository<Page> _pageRepository;

    public GetPageByIdQueryHandler(IRepository<Page> pageRepository)
    {
        _pageRepository = pageRepository;
    }

    public async Task<PageDto?> Handle(GetPageByIdQuery request, CancellationToken cancellationToken)
    {
        var page = await _pageRepository.GetByIdAsync(request.Id);
        
        if (page == null || page.SiteId != request.SiteId || page.IsDeleted)
            return null;

        return page.ToDto();
    }
}

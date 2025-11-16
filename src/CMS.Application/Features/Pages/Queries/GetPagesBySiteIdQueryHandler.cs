using MediatR;
using CMS.Application.Mappings;
using CMS.Application.DTOs;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.Pages.Queries;

public class GetPagesBySiteIdQueryHandler : IRequestHandler<GetPagesBySiteIdQuery, IEnumerable<PageDto>>
{
    private readonly IRepository<Page> _pageRepository;

    public GetPagesBySiteIdQueryHandler(IRepository<Page> pageRepository)
    {
        _pageRepository = pageRepository;
    }

    public async Task<IEnumerable<PageDto>> Handle(GetPagesBySiteIdQuery request, CancellationToken cancellationToken)
    {
        var pages = await _pageRepository.FindAsync(p => p.SiteId == request.SiteId && !p.IsDeleted);
        return pages.ToDto();
    }
}

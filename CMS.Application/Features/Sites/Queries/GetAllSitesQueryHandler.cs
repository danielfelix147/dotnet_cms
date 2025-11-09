using MediatR;
using CMS.Application.DTOs;
using CMS.Application.Mappings;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.Sites.Queries;

public class GetAllSitesQueryHandler : IRequestHandler<GetAllSitesQuery, IEnumerable<SiteDto>>
{
    private readonly ISiteRepository _siteRepository;

    public GetAllSitesQueryHandler(ISiteRepository siteRepository)
    {
        _siteRepository = siteRepository;
    }

    public async Task<IEnumerable<SiteDto>> Handle(GetAllSitesQuery request, CancellationToken cancellationToken)
    {
        var sites = await _siteRepository.GetAllAsync();
        return sites.ToDto();
    }
}

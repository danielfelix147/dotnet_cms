using MediatR;
using CMS.Application.DTOs;
using CMS.Application.Mappings;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.Sites.Queries;

public class GetSiteByIdQueryHandler : IRequestHandler<GetSiteByIdQuery, SiteDto?>
{
    private readonly ISiteRepository _siteRepository;

    public GetSiteByIdQueryHandler(ISiteRepository siteRepository)
    {
        _siteRepository = siteRepository;
    }

    public async Task<SiteDto?> Handle(GetSiteByIdQuery request, CancellationToken cancellationToken)
    {
        var site = await _siteRepository.GetByIdAsync(request.SiteId);
        return site?.ToDto();
    }
}

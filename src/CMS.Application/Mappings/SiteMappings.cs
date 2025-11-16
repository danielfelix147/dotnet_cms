using CMS.Application.DTOs;
using CMS.Domain.Entities;

namespace CMS.Application.Mappings;

public static class SiteMappings
{
    public static SiteDto ToDto(this Site site)
    {
        return new SiteDto
        {
            Id = site.Id,
            Name = site.Name,
            Domain = site.Domain,
            Description = site.Description,
            IsActive = site.IsActive
        };
    }

    public static List<SiteDto> ToDto(this IEnumerable<Site> sites)
    {
        return sites.Select(s => s.ToDto()).ToList();
    }
}

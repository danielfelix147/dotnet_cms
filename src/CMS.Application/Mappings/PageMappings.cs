using CMS.Application.DTOs;
using CMS.Domain.Entities;

namespace CMS.Application.Mappings;

public static class PageMappings
{
    public static PageDto ToDto(this Page page)
    {
        return new PageDto
        {
            Id = page.Id,
            SiteId = page.SiteId,
            PageId = page.PageId,
            Title = page.Title,
            Description = page.Description,
            IsPublished = page.IsPublished
        };
    }

    public static List<PageDto> ToDto(this IEnumerable<Page> pages)
    {
        return pages.Select(p => p.ToDto()).ToList();
    }
}

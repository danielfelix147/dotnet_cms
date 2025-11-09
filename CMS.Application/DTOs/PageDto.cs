namespace CMS.Application.DTOs;

public class PageDto
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public string PageId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublished { get; set; }
    public List<PageContentDto> Contents { get; set; } = new();
}

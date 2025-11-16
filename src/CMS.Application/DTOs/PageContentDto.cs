namespace CMS.Application.DTOs;

public class PageContentDto
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public string ContentId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Order { get; set; }
}

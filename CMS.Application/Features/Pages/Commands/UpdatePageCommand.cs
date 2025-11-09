using MediatR;
using CMS.Application.DTOs;

namespace CMS.Application.Features.Pages.Commands;

public class UpdatePageCommand : IRequest<PageDto?>
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public string PageId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublished { get; set; }
}

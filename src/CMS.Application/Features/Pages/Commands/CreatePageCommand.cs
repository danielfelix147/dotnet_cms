using MediatR;
using CMS.Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace CMS.Application.Features.Pages.Commands;

public class CreatePageCommand : IRequest<PageDto>
{
    [Required]
    public Guid SiteId { get; set; }
    
    [Required(ErrorMessage = "PageId is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "PageId must be between 1 and 100 characters")]
    [RegularExpression(@"^[a-zA-Z0-9-_]+$", ErrorMessage = "PageId can only contain alphanumeric characters, hyphens, and underscores")]
    public string PageId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
    
    public bool IsPublished { get; set; }
}

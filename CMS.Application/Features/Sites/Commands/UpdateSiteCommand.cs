using MediatR;
using CMS.Application.DTOs;

namespace CMS.Application.Features.Sites.Commands;

public class UpdateSiteCommand : IRequest<SiteDto?>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

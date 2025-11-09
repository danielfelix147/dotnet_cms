using MediatR;
using CMS.Application.DTOs;

namespace CMS.Application.Features.Sites.Commands;

public class CreateSiteCommand : IRequest<SiteDto>
{
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string? Description { get; set; }
}

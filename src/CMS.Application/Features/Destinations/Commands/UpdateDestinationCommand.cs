using MediatR;
using CMS.Application.DTOs;

namespace CMS.Application.Features.Destinations.Commands;

public class UpdateDestinationCommand : IRequest<DestinationDto?>
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public string DestinationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPublished { get; set; }
}

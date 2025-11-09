using MediatR;

namespace CMS.Application.Features.Destinations.Commands;

public class DeleteDestinationCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
}

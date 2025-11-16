using MediatR;

namespace CMS.Application.Features.Tours.Commands;

public class DeleteTourCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public Guid DestinationId { get; set; }
}

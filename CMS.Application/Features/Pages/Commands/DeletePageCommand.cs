using MediatR;

namespace CMS.Application.Features.Pages.Commands;

public class DeletePageCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
}

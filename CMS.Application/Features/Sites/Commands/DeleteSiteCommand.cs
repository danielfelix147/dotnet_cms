using MediatR;

namespace CMS.Application.Features.Sites.Commands;

public class DeleteSiteCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

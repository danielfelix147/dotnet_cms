using MediatR;

namespace CMS.Application.Features.Products.Commands;

public class DeleteProductCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
}

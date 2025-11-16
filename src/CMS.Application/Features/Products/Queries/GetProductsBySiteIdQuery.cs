using MediatR;
using CMS.Application.DTOs;

namespace CMS.Application.Features.Products.Queries;

public class GetProductsBySiteIdQuery : IRequest<IEnumerable<ProductDto>>
{
    public Guid SiteId { get; set; }
}

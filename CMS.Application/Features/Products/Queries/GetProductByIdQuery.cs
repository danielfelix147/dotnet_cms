using MediatR;
using CMS.Application.DTOs;

namespace CMS.Application.Features.Products.Queries;

public class GetProductByIdQuery : IRequest<ProductDto?>
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
}

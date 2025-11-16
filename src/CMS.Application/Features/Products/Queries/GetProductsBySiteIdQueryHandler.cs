using MediatR;
using CMS.Application.Mappings;
using CMS.Application.DTOs;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;

namespace CMS.Application.Features.Products.Queries;

public class GetProductsBySiteIdQueryHandler : IRequestHandler<GetProductsBySiteIdQuery, IEnumerable<ProductDto>>
{
    private readonly IRepository<Product> _productRepository;
public GetProductsBySiteIdQueryHandler(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
}

    public async Task<IEnumerable<ProductDto>> Handle(GetProductsBySiteIdQuery request, CancellationToken cancellationToken)
    {
        var products = await _productRepository.FindAsync(p => p.SiteId == request.SiteId && !p.IsDeleted);
        return products.ToDto();
    }
}

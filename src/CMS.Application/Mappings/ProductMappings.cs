using CMS.Application.DTOs;
using CMS.Domain.Entities;

namespace CMS.Application.Mappings;

public static class ProductMappings
{
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            ProductId = product.ProductId,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            IsPublished = product.IsPublished
        };
    }

    public static List<ProductDto> ToDto(this IEnumerable<Product> products)
    {
        return products.Select(p => p.ToDto()).ToList();
    }
}

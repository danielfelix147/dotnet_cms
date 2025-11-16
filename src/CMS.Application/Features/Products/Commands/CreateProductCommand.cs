using MediatR;
using CMS.Application.DTOs;

namespace CMS.Application.Features.Products.Commands;

public class CreateProductCommand : IRequest<ProductDto>
{
    public Guid SiteId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsPublished { get; set; }
}

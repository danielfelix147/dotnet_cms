namespace CMS.Application.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsPublished { get; set; }
    public List<ImageDto> Images { get; set; } = new();
    public List<FileDto> Files { get; set; } = new();
}

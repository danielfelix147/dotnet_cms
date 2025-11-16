namespace CMS.Application.DTOs;

public class SiteExportDto
{
    public Guid SiteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public DateTime ExportedAt { get; set; }
    
    public List<PageExportDto> Pages { get; set; } = new();
    public List<ProductExportDto> Products { get; set; } = new();
    public List<DestinationExportDto> Destinations { get; set; } = new();
    public List<MediaDto> Media { get; set; } = new();
}

public class PageExportDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public List<PageContentExportDto> Contents { get; set; } = new();
}

public class PageContentExportDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

public class ProductExportDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
}

public class DestinationExportDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public List<TourExportDto> Tours { get; set; } = new();
}

public class TourExportDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Duration { get; set; }
}

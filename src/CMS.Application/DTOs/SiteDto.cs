namespace CMS.Application.DTOs;

public class SiteDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

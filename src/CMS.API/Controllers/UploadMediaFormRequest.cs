namespace CMS.API.Controllers;

public class UploadMediaFormRequest
{
    public IFormFile File { get; set; } = null!;
    public Guid SiteId { get; set; }
    public string? Description { get; set; }
}

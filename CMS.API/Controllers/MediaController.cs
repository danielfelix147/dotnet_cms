using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CMS.Application.Features.Media.Commands;
using CMS.Application.Features.Media.Queries;
using CMS.Application.DTOs;

namespace CMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IMediator _mediator;

    public MediaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Admin,Editor")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadMedia([FromForm] UploadMediaFormRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest("No file uploaded");

        // Read file data
        using var memoryStream = new MemoryStream();
        await request.File.CopyToAsync(memoryStream);
        var fileData = memoryStream.ToArray();

        var command = new UploadMediaCommand
        {
            SiteId = request.SiteId,
            FileName = request.File.FileName,
            ContentType = request.File.ContentType,
            FileSize = request.File.Length,
            FileData = fileData,
            Description = request.Description
        };

        var mediaId = await _mediator.Send(command);
        
        return Ok(new { id = mediaId, message = "File uploaded successfully" });
    }

    [HttpGet("site/{siteId}")]
    public async Task<ActionResult<IEnumerable<MediaDto>>> GetMediaBySite(Guid siteId)
    {
        var query = new GetMediaBySiteIdQuery { SiteId = siteId };
        var media = await _mediator.Send(query);
        return Ok(media);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> DeleteMedia(Guid id, [FromQuery] string mediaType = "Image")
    {
        var command = new DeleteMediaCommand 
        { 
            Id = id,
            MediaType = mediaType
        };
        
        var result = await _mediator.Send(command);
        
        if (!result)
            return NotFound();
            
        return NoContent();
    }
}

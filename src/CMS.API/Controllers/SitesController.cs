using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CMS.Application.Features.Sites.Commands;
using CMS.Application.Features.Sites.Queries;
using CMS.Application.Features.SiteUsers.Commands;
using CMS.Application.Features.SiteUsers.Queries;
using CMS.Application.DTOs;

namespace CMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class SitesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SitesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllSitesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetSiteByIdQuery { SiteId = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateSiteCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSiteCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        var result = await _mediator.Send(command);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteSiteCommand { Id = id };
        var result = await _mediator.Send(command);
        
        if (!result)
            return NotFound();
            
        return NoContent();
    }

    [HttpGet("{siteId}/users")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetSiteUsers(Guid siteId)
    {
        var users = await _mediator.Send(new GetSiteUsersQuery { SiteId = siteId });
        return Ok(users);
    }

    [HttpPost("{siteId}/users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignUserToSite(Guid siteId, [FromBody] AssignUserToSiteRequest request)
    {
        var result = await _mediator.Send(new AssignUserToSiteCommand 
        { 
            SiteId = siteId, 
            UserId = request.UserId, 
            Role = request.Role 
        });
        if (!result)
            return BadRequest("Failed to assign user to site.");
        return Ok();
    }

    [HttpDelete("{siteId}/users/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveUserFromSite(Guid siteId, string userId)
    {
        var result = await _mediator.Send(new RemoveUserFromSiteCommand 
        { 
            SiteId = siteId, 
            UserId = userId 
        });
        if (!result)
            return NotFound();
        return NoContent();
    }
}

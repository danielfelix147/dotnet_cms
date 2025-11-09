using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CMS.Application.Features.Destinations.Commands;
using CMS.Application.Features.Destinations.Queries;
using CMS.Application.Features.Tours.Commands;
using CMS.Application.Features.Tours.Queries;

namespace CMS.API.Controllers;

[ApiController]
[Route("api/sites/{siteId}/destinations")]
public class DestinationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DestinationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid siteId)
    {
        var query = new GetDestinationsBySiteIdQuery { SiteId = siteId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid siteId, Guid id)
    {
        var query = new GetDestinationByIdQuery { Id = id, SiteId = siteId };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> Create(Guid siteId, [FromBody] CreateDestinationCommand command)
    {
        command.SiteId = siteId;
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { siteId, id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> Update(Guid siteId, Guid id, [FromBody] UpdateDestinationCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        command.SiteId = siteId;
        var result = await _mediator.Send(command);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> Delete(Guid siteId, Guid id)
    {
        var command = new DeleteDestinationCommand { Id = id, SiteId = siteId };
        var result = await _mediator.Send(command);
        
        if (!result)
            return NotFound();
            
        return NoContent();
    }

    // Tours nested under destinations
    [HttpGet("{destinationId}/tours")]
    public async Task<IActionResult> GetTours(Guid siteId, Guid destinationId)
    {
        var query = new GetToursByDestinationIdQuery { DestinationId = destinationId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{destinationId}/tours")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> CreateTour(Guid siteId, Guid destinationId, [FromBody] CreateTourCommand command)
    {
        command.DestinationId = destinationId;
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTours), new { siteId, destinationId }, result);
    }

    [HttpPut("{destinationId}/tours/{tourId}")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> UpdateTour(Guid siteId, Guid destinationId, Guid tourId, [FromBody] UpdateTourCommand command)
    {
        if (tourId != command.Id)
            return BadRequest("ID mismatch");

        command.DestinationId = destinationId;
        var result = await _mediator.Send(command);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpDelete("{destinationId}/tours/{tourId}")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> DeleteTour(Guid siteId, Guid destinationId, Guid tourId)
    {
        var command = new DeleteTourCommand { Id = tourId, DestinationId = destinationId };
        var result = await _mediator.Send(command);
        
        if (!result)
            return NotFound();
            
        return NoContent();
    }
}

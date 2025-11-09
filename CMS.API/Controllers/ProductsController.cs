using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CMS.Application.Features.Products.Commands;
using CMS.Application.Features.Products.Queries;

namespace CMS.API.Controllers;

[ApiController]
[Route("api/sites/{siteId}/products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid siteId)
    {
        var query = new GetProductsBySiteIdQuery { SiteId = siteId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid siteId, Guid id)
    {
        var query = new GetProductByIdQuery { Id = id, SiteId = siteId };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> Create(Guid siteId, [FromBody] CreateProductCommand command)
    {
        command.SiteId = siteId;
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { siteId, id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> Update(Guid siteId, Guid id, [FromBody] UpdateProductCommand command)
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
        var command = new DeleteProductCommand { Id = id, SiteId = siteId };
        var result = await _mediator.Send(command);
        
        if (!result)
            return NotFound();
            
        return NoContent();
    }
}

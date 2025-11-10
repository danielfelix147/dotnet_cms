using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CMS.Domain.Plugins;
using MediatR;
using CMS.Application.Features.Content.Queries;
using CMS.Application.DTOs;

namespace CMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class ContentController : ControllerBase
{
    private readonly IPluginManager _pluginManager;
    private readonly IMediator _mediator;

    public ContentController(IPluginManager pluginManager, IMediator mediator)
    {
        _pluginManager = pluginManager;
        _mediator = mediator;
    }

    [HttpGet("site/{siteId}")]
    public async Task<IActionResult> GetSiteContent(Guid siteId)
    {
        var json = await _pluginManager.GenerateSiteJsonAsync(siteId);
        return Content(json, "application/json");
    }

    [HttpGet("export/{siteId}")]
    public async Task<ActionResult<SiteExportDto>> ExportSiteContent(Guid siteId)
    {
        try
        {
            var query = new ExportSiteContentQuery { SiteId = siteId };
            var export = await _mediator.Send(query);
            return Ok(export);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("site/{siteId}/plugin/{pluginSystemName}")]
    public async Task<IActionResult> GetPluginContent(Guid siteId, string pluginSystemName)
    {
        var plugin = _pluginManager.GetPlugin(pluginSystemName);
        
        if (plugin == null)
            return NotFound($"Plugin '{pluginSystemName}' not found");
            
        if (plugin is IContentPlugin contentPlugin)
        {
            var json = await contentPlugin.GenerateJsonAsync(siteId);
            return Content(json, "application/json");
        }
        
        return BadRequest("Plugin does not support content generation");
    }
}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Domain.Entities;
using CMS.Domain.Interfaces;
using CMS.Domain.Plugins;
using CMS.Application.DTOs;

namespace CMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PluginsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPluginManager _pluginManager;

    public PluginsController(IUnitOfWork unitOfWork, IPluginManager pluginManager)
    {
        _unitOfWork = unitOfWork;
        _pluginManager = pluginManager;
    }

    /// <summary>
    /// Get all registered plugins from DI container
    /// </summary>
    [HttpGet]
    public IActionResult GetAllPlugins()
    {
        var plugins = _pluginManager.GetAllPlugins();
        var pluginDtos = plugins.Select(p => new
        {
            SystemName = p.SystemName,
            DisplayName = p.DisplayName,
            Description = p.Description,
            Version = p.Version
        });
        return Ok(pluginDtos);
    }

    /// <summary>
    /// Get all plugins from database
    /// </summary>
    [HttpGet("database")]
    public async Task<IActionResult> GetDatabasePlugins()
    {
        var plugins = await _unitOfWork.Repository<Plugin>().GetAllAsync();
        var pluginDtos = plugins
            .Where(p => !p.IsDeleted)
            .Select(p => new PluginDto
            {
                Id = p.Id,
                Name = p.Name,
                SystemName = p.SystemName,
                Description = p.Description,
                IsActive = p.IsActive
            });
        return Ok(pluginDtos);
    }

    /// <summary>
    /// Get a specific plugin by system name
    /// </summary>
    [HttpGet("{systemName}")]
    public IActionResult GetPluginBySystemName(string systemName)
    {
        var plugin = _pluginManager.GetPlugin(systemName);
        
        if (plugin == null)
            return NotFound(new { message = $"Plugin '{systemName}' not found" });

        return Ok(new
        {
            SystemName = plugin.SystemName,
            DisplayName = plugin.DisplayName,
            Description = plugin.Description,
            Version = plugin.Version
        });
    }

    /// <summary>
    /// Get plugins enabled for a specific site
    /// </summary>
    [HttpGet("site/{siteId}")]
    public async Task<IActionResult> GetSitePlugins(Guid siteId)
    {
        var sitePlugins = await _unitOfWork.Repository<SitePlugin>()
            .FindAsync(sp => sp.SiteId == siteId && !sp.IsDeleted);

        var pluginIds = sitePlugins.Select(sp => sp.PluginId).ToList();
        var plugins = await _unitOfWork.Repository<Plugin>()
            .FindAsync(p => pluginIds.Contains(p.Id) && !p.IsDeleted);

        var result = sitePlugins.Select(sp =>
        {
            var plugin = plugins.FirstOrDefault(p => p.Id == sp.PluginId);
            return new
            {
                Id = sp.Id,
                PluginId = sp.PluginId,
                PluginName = plugin?.Name,
                SystemName = plugin?.SystemName,
                IsEnabled = sp.IsEnabled,
                Configuration = sp.Configuration
            };
        });

        return Ok(result);
    }

    /// <summary>
    /// Enable a plugin for a site
    /// </summary>
    [HttpPost("site/{siteId}/enable/{pluginId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EnablePlugin(Guid siteId, Guid pluginId, [FromBody] PluginConfigurationRequest? request = null)
    {
        // Verify site exists
        var site = await _unitOfWork.Repository<Site>().GetByIdAsync(siteId);
        if (site == null || site.IsDeleted)
            return NotFound(new { message = "Site not found" });

        // Verify plugin exists
        var plugin = await _unitOfWork.Repository<Plugin>().GetByIdAsync(pluginId);
        if (plugin == null || plugin.IsDeleted)
            return NotFound(new { message = "Plugin not found" });

        // Check if already enabled
        var existingSitePlugin = (await _unitOfWork.Repository<SitePlugin>()
            .FindAsync(sp => sp.SiteId == siteId && sp.PluginId == pluginId && !sp.IsDeleted))
            .FirstOrDefault();

        if (existingSitePlugin != null)
        {
            existingSitePlugin.IsEnabled = true;
            existingSitePlugin.Configuration = request?.Configuration;
            existingSitePlugin.UpdatedAt = DateTime.UtcNow;
            existingSitePlugin.UpdatedBy = User.Identity?.Name ?? "system";
            await _unitOfWork.Repository<SitePlugin>().UpdateAsync(existingSitePlugin);
        }
        else
        {
            var sitePlugin = new SitePlugin
            {
                Id = Guid.NewGuid(),
                SiteId = siteId,
                PluginId = pluginId,
                IsEnabled = true,
                Configuration = request?.Configuration,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name ?? "system"
            };
            await _unitOfWork.Repository<SitePlugin>().AddAsync(sitePlugin);
        }

        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Plugin enabled successfully" });
    }

    /// <summary>
    /// Disable a plugin for a site
    /// </summary>
    [HttpPost("site/{siteId}/disable/{pluginId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DisablePlugin(Guid siteId, Guid pluginId)
    {
        var sitePlugin = (await _unitOfWork.Repository<SitePlugin>()
            .FindAsync(sp => sp.SiteId == siteId && sp.PluginId == pluginId && !sp.IsDeleted))
            .FirstOrDefault();

        if (sitePlugin == null)
            return NotFound(new { message = "Plugin is not associated with this site" });

        sitePlugin.IsEnabled = false;
        sitePlugin.UpdatedAt = DateTime.UtcNow;
        sitePlugin.UpdatedBy = User.Identity?.Name ?? "system";
        
        await _unitOfWork.Repository<SitePlugin>().UpdateAsync(sitePlugin);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Plugin disabled successfully" });
    }

    /// <summary>
    /// Update plugin configuration for a site
    /// </summary>
    [HttpPut("site/{siteId}/plugin/{pluginId}/config")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePluginConfiguration(Guid siteId, Guid pluginId, [FromBody] PluginConfigurationRequest request)
    {
        var sitePlugin = (await _unitOfWork.Repository<SitePlugin>()
            .FindAsync(sp => sp.SiteId == siteId && sp.PluginId == pluginId && !sp.IsDeleted))
            .FirstOrDefault();

        if (sitePlugin == null)
            return NotFound(new { message = "Plugin is not associated with this site" });

        sitePlugin.Configuration = request.Configuration;
        sitePlugin.UpdatedAt = DateTime.UtcNow;
        sitePlugin.UpdatedBy = User.Identity?.Name ?? "system";
        
        await _unitOfWork.Repository<SitePlugin>().UpdateAsync(sitePlugin);
        await _unitOfWork.SaveChangesAsync();

        return Ok(new { message = "Plugin configuration updated successfully" });
    }

    /// <summary>
    /// Sync plugins from DI container to database (Admin utility)
    /// </summary>
    [HttpPost("sync")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SyncPlugins()
    {
        var registeredPlugins = _pluginManager.GetAllPlugins();
        var databasePlugins = await _unitOfWork.Repository<Plugin>().GetAllAsync();
        var databasePluginDict = databasePlugins.ToDictionary(p => p.SystemName, p => p);

        var addedCount = 0;
        var updatedCount = 0;

        foreach (var plugin in registeredPlugins)
        {
            if (databasePluginDict.TryGetValue(plugin.SystemName, out var dbPlugin))
            {
                // Update existing
                if (dbPlugin.Name != plugin.DisplayName || 
                    dbPlugin.Description != plugin.Description)
                {
                    dbPlugin.Name = plugin.DisplayName;
                    dbPlugin.Description = plugin.Description;
                    dbPlugin.UpdatedAt = DateTime.UtcNow;
                    dbPlugin.UpdatedBy = User.Identity?.Name ?? "system";
                    await _unitOfWork.Repository<Plugin>().UpdateAsync(dbPlugin);
                    updatedCount++;
                }
            }
            else
            {
                // Add new
                var newPlugin = new Plugin
                {
                    Id = Guid.NewGuid(),
                    Name = plugin.DisplayName,
                    SystemName = plugin.SystemName,
                    Description = plugin.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = User.Identity?.Name ?? "system"
                };
                await _unitOfWork.Repository<Plugin>().AddAsync(newPlugin);
                addedCount++;
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return Ok(new 
        { 
            message = "Plugins synchronized successfully",
            added = addedCount,
            updated = updatedCount,
            total = registeredPlugins.Count()
        });
    }
}

public class PluginConfigurationRequest
{
    public string? Configuration { get; set; }
}

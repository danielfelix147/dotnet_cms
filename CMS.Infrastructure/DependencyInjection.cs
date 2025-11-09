using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CMS.Domain.Interfaces;
using CMS.Domain.Plugins;
using CMS.Infrastructure.Data;
using CMS.Infrastructure.Repositories;
using CMS.Infrastructure.Plugins;
using CMS.Infrastructure.Services;
using CMS.Application.Interfaces;
using CMS.Application.Services;

namespace CMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<CMSDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Add repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISiteRepository, SiteRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Add plugins
        services.AddScoped<IPlugin, PageManagementPlugin>();
        services.AddScoped<IPlugin, ProductManagementPlugin>();
        services.AddScoped<IPlugin, TravelManagementPlugin>();
        services.AddScoped<IPluginManager, PluginManager>();

        // Add services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}

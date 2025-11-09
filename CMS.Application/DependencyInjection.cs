using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using CMS.Application.Services;

namespace CMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        
        // Register security services
        services.AddSingleton<IHtmlSanitizerService, HtmlSanitizerService>();
        
        return services;
    }
}

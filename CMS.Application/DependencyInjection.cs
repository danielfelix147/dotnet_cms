using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using FluentValidation;
using CMS.Application.Services;
using CMS.Application.Common.Behaviors;

namespace CMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        
        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);
        
        // Register validation behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        
        // Register security services
        services.AddSingleton<IHtmlSanitizerService, HtmlSanitizerService>();
        
        return services;
    }
}

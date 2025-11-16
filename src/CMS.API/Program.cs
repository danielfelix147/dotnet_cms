using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using AspNetCoreRateLimit;
using CMS.Application;
using CMS.Infrastructure;
using CMS.Infrastructure.Data;
using CMS.Domain.Entities;
using CMS.Domain.Plugins;
using CMS.API;
using CMS.API.Middleware;
using CMS.API.Filters;
using FluentValidation;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/cms-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting CMS API");

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/cms-.log", rollingInterval: RollingInterval.Day));


// Add services to the container
builder.Services.AddControllers(options =>
{
    // Enable automatic model validation to prevent invalid data
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = false;
    
    // Add global validation exception filter
    options.Filters.Add<ValidationExceptionFilter>();
});

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CMS.Application.Features.Auth.Validators.RegisterRequestValidator>();

// Add anti-forgery token support
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT Bearer authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "CMS API", 
        Version = "v1",
        Description = "Content Management System API with JWT Authentication"
    });

    // Add JWT Bearer authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Enable file upload support in Swagger
    c.OperationFilter<FileUploadOperationFilter>();
});

// Add Application and Infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<CMSDbContext>()
    .AddDefaultTokenProviders();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? ""))
    };
});

builder.Services.AddAuthorization();

// Add Output Caching
builder.Services.AddOutputCache(options =>
{
    // Default cache profile for all requests
    options.AddBasePolicy(builder => builder
        .Expire(TimeSpan.FromMinutes(5))
        .Tag("default"));
    
    // Cache policy for GET requests
    options.AddPolicy("GetCache", builder => builder
        .Expire(TimeSpan.FromMinutes(10))
        .SetVaryByQuery("*")
        .Tag("get-requests"));
    
    // Cache policy for site-specific data
    options.AddPolicy("SiteCache", builder => builder
        .Expire(TimeSpan.FromMinutes(15))
        .SetVaryByRouteValue("siteId")
        .Tag("site-data"));
});

// Add Health Checks
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString ?? "Host=localhost;Port=5432;Database=CMS_DB;Username=postgres;Password=postgres",
        name: "database",
        tags: new[] { "db", "postgresql" })
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: new[] { "api" });

// Configure Rate Limiting
builder.Services.AddMemoryCache();

// Read rate limits from configuration (allows different limits for testing)
var rateLimitingEnabled = builder.Configuration.GetValue<bool>("RateLimiting:Enabled", true);
var loginLimit = builder.Configuration.GetValue<int>("RateLimiting:Login:Limit", 5);
var loginPeriod = builder.Configuration.GetValue<string>("RateLimiting:Login:Period", "1m");
var registerLimit = builder.Configuration.GetValue<int>("RateLimiting:Register:Limit", 3);
var registerPeriod = builder.Configuration.GetValue<string>("RateLimiting:Register:Period", "1h");
var generalPerMinute = builder.Configuration.GetValue<int>("RateLimiting:General:PerMinute", 100);
var generalPerHour = builder.Configuration.GetValue<int>("RateLimiting:General:PerHour", 1000);

builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = rateLimitingEnabled;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.RealIpHeader = "X-Real-IP";
    options.ClientIdHeader = "X-ClientId";
    options.GeneralRules = new List<RateLimitRule>
    {
        // Strict limits on authentication endpoints
        new RateLimitRule
        {
            Endpoint = "POST:/api/auth/login",
            Period = loginPeriod,
            Limit = loginLimit
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/auth/register",
            Period = registerPeriod,
            Limit = registerLimit
        },
        // General API rate limit
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = generalPerMinute
        },
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1h",
            Limit = generalPerHour
        }
    };
});

// Log rate limiting configuration
Console.WriteLine("====================================");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine("Rate Limiting Configuration:");
Console.WriteLine($"  Login: {loginLimit} requests per {loginPeriod}");
Console.WriteLine($"  Register: {registerLimit} requests per {registerPeriod}");
Console.WriteLine($"  General: {generalPerMinute} requests per minute, {generalPerHour} per hour");
Console.WriteLine("====================================");

builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// Add CORS with secure configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("SecureCorsPolicy", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
            ?? new[] { "http://localhost:3000", "http://localhost:5173" }; // Default for dev
        
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
    
    // Keep a permissive policy for development only
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("DevelopmentCorsPolicy", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    }
});

var app = builder.Build();

// Seed roles, default admin user, and plugins
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var context = scope.ServiceProvider.GetRequiredService<CMSDbContext>();
    var pluginManager = scope.ServiceProvider.GetRequiredService<IPluginManager>();
    
    // Create roles
    var roles = new[] { "Admin", "Editor", "Viewer" };
    
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    
    // Create default admin user
    var adminEmail = "admin@cms.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        
        var result = await userManager.CreateAsync(adminUser, "Admin@123");
        
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
    
    // Seed plugins from registered plugins in DI
    var registeredPlugins = pluginManager.GetAllPlugins();
    var existingPlugins = await context.Plugins.ToListAsync();
    var existingSystemNames = existingPlugins.Select(p => p.SystemName).ToHashSet();
    
    foreach (var plugin in registeredPlugins)
    {
        if (!existingSystemNames.Contains(plugin.SystemName))
        {
            var dbPlugin = new Plugin
            {
                Id = Guid.NewGuid(),
                Name = plugin.DisplayName,
                SystemName = plugin.SystemName,
                Description = plugin.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };
            context.Plugins.Add(dbPlugin);
        }
    }
    
    await context.SaveChangesAsync();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection(); // Only redirect to HTTPS in production
}

// Apply security headers to all requests
app.UseMiddleware<SecurityHeadersMiddleware>();

// Apply rate limiting before authentication
app.UseIpRateLimiting();

// Use CORS before authentication - this must come early to handle preflight requests
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentCorsPolicy"); // Permissive CORS for development
}
else
{
    app.UseCors("SecureCorsPolicy"); // Secure CORS for production
}

app.UseStaticFiles(); // Enable static file serving for uploads

// Use output caching
app.UseOutputCache();

// Map health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false, // No checks, just returns if the app is running
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Log.Information("CMS API started successfully");
app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program accessible to test projects
public partial class Program { }

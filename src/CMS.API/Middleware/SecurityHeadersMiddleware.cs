namespace CMS.API.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Prevent clickjacking
        context.Response.Headers["X-Frame-Options"] = "DENY";
        
        // Prevent MIME-type sniffing
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        
        // XSS Protection (legacy but still useful for older browsers)
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        
        // Content Security Policy - restrictive by default
        if (!context.Request.Path.StartsWithSegments("/swagger"))
        {
            context.Response.Headers["Content-Security-Policy"] = 
                "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self';";
        }
        
        // Referrer Policy - don't leak referrer info
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        
        // Permissions Policy - disable unnecessary features
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=(), payment=()";
        
        // HSTS (only in production with HTTPS)
        if (!context.Request.Host.Host.Contains("localhost") && context.Request.IsHttps)
        {
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
        }

        await _next(context);
    }
}

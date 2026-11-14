using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace EducationPortal.Web.Infrastructure;

/// <summary>
/// Adds the security headers from SEC-WEB-04 to every response. The Content-Security-Policy allows scripts only from
/// the application itself. Inline styles stay allowed, because Angular injects component styles at runtime.
/// </summary>
public static class SecurityHeaders
{
    private const string ContentSecurityPolicy =
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "font-src 'self'; " +
        "connect-src 'self'; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'; " +
        "frame-ancestors 'none'";

    private const string PermissionsPolicy =
        "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()";

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            // The headers are added just before the response starts, so they stay on every response, including the
            // ones an exception handler writes after clearing the response.
            context.Response.OnStarting(static state =>
            {
                var httpContext = (HttpContext)state;
                var headers = httpContext.Response.Headers;

                headers["Content-Security-Policy"] = ContentSecurityPolicy;
                headers["X-Content-Type-Options"] = "nosniff";
                headers["X-Frame-Options"] = "DENY";
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                headers["Permissions-Policy"] = PermissionsPolicy;

                // API responses can hold personal data, so browsers and proxies must not keep them.
                if (httpContext.Request.Path.StartsWithSegments("/api"))
                {
                    headers["Cache-Control"] = "no-store";
                }

                return Task.CompletedTask;
            }, context);

            await next();
        });
    }
}

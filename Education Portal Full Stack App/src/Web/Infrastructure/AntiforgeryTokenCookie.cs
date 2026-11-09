using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EducationPortal.Web.Infrastructure;

/// <summary>
/// Gives Angular the antiforgery request token in a cookie that script can read (SEC-WEB-03). Angular copies the cookie
/// into the X-XSRF-TOKEN header of every unsafe request. The server checks that header against its own antiforgery
/// cookie, which script cannot read.
/// </summary>
public static class AntiforgeryTokenCookie
{
    public const string CookieName = "__Host-XSRF-TOKEN";
    public const string HeaderName = "X-XSRF-TOKEN";

    public static IApplicationBuilder UseAntiforgeryTokenCookie(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var tokens = context.RequestServices.GetRequiredService<IAntiforgery>().GetAndStoreTokens(context);

            context.Response.Cookies.Append(CookieName, tokens.RequestToken!, new CookieOptions
            {
                HttpOnly = false, // Angular reads this cookie.
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                IsEssential = true,
            });

            await next();
        });
    }
}

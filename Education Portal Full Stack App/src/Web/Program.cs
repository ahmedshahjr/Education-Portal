using EducationPortal.Infrastructure.Data;
using EducationPortal.Web.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();

builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    // HSTS for one year, including subdomains (SEC-WEB-01). Development keeps plain HTTP for local tools.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseExceptionHandler(options => { });
app.UseSecurityHeaders();
app.UseRateLimiter();

// The Angular files are static and anonymous (SEC-AUTHN-01).
app.UseFileServer();

// Authentication and authorization run after the security headers, so a 401 or 403 carries them too. Without these
// calls, WebApplication inserts both middlewares straight after routing, which is before the headers and the token.
app.UseAuthentication();
app.UseAntiforgeryTokenCookie();
app.UseAuthorization();
app.UseAntiforgery();

if (app.Environment.IsDevelopment())
{
    // The OpenAPI document and the API reference exist only in Development (SEC-WEB-05).
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapDefaultEndpoints();
app.MapEndpoints(typeof(Program).Assembly);

// compliance-exception: SEC-AUTHN-01 serves the Angular shell, a static page without data
app.MapFallbackToFile("index.html").AllowAnonymous();

app.Run();

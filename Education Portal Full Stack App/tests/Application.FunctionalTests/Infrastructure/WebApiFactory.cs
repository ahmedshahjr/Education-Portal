using EducationPortal.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EducationPortal.Application.FunctionalTests.Infrastructure;

/// <summary>
/// The Development host. It creates and seeds the schema when it starts, so it runs first. Its sign-in limit is raised,
/// because every test signs in from the same address; <see cref="Hosting.SignInRateLimitTests"/> checks the real limit.
/// </summary>
public class WebApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseSetting("ConnectionStrings:EducationPortalDb", connectionString)
            .UseSetting("RateLimiting:SignInPermitLimit", "1000")
            // Tests run the lifecycle by hand at a chosen time (SubscriptionLifecycleTests), never on the clock.
            .UseSetting("SubscriptionLifecycle:Enabled", "false");

        builder.ConfigureTestServices(services =>
        {
            services
                .RemoveAll<IUser>()
                .AddScoped<IUser, TestUser>();
        });
    }
}

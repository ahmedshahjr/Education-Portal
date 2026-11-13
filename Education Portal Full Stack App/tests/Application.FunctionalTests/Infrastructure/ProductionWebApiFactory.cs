using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace EducationPortal.Application.FunctionalTests.Infrastructure;

/// <summary>
/// A host in Production mode. It does not reset or seed the database, does not map the API reference, and keeps the
/// configured sign-in limit. It runs against a schema that the Development host has already created.
/// </summary>
public class ProductionWebApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseEnvironment("Production")
            .UseSetting("ConnectionStrings:EducationPortalDb", connectionString)
            .UseSetting("SubscriptionLifecycle:Enabled", "false");
    }
}

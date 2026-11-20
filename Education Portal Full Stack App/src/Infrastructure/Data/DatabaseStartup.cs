using EducationPortal.Infrastructure.Data.Locations;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EducationPortal.Infrastructure.Data;

public static class DatabaseStartupExtensions
{
    /// <summary>
    /// Development start-up: applies the EF Core migrations (the schema survives restarts, SEC-CFG-03), imports the
    /// location data and seeds the demo tenants. Nothing is dropped. Outside Development the schema is changed only by a
    /// reviewed migration script, so this throws there instead of migrating.
    /// </summary>
    public static async Task MigrateAndSeedDevelopmentDatabaseAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            throw new InvalidOperationException("Migrations and the development seed run only in the Development environment.");
        }

        using var scope = app.Services.CreateScope();

        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();

        // The location data comes before the demo seed, which assigns its schools to the imported countries.
        await scope.ServiceProvider.GetRequiredService<LocationDataImporter>().ImportAsync();

        await scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>().SeedAsync();
    }

    /// <summary>
    /// Imports the built-in countries, states and cities (<see cref="LocationDataImporter"/>). Runs on every start-up outside
    /// Development; safe to run again. The schema must already exist.
    /// </summary>
    public static async Task ImportLocationDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        await scope.ServiceProvider.GetRequiredService<LocationDataImporter>().ImportAsync();
    }
}

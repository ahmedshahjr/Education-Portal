using EducationPortal.Infrastructure.Data;
using EducationPortal.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EducationPortal.Web.Infrastructure;

/// <summary>
/// Lets <c>dotnet ef migrations add</c> build the model without starting the app (no Aspire, no secret store, no database
/// connection). The migration tools only read the model, so the connection string is a placeholder that is never opened.
/// Runtime uses the connection string from AppHost or the secret store.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=educationportal_design")
            .Options;

        return new ApplicationDbContext(options, new CurrentTenant(), new EducationPortal.Infrastructure.Data.Tenancy.TenantSystemScope());
    }
}

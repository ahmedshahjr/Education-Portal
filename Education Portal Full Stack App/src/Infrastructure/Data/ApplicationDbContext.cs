using System.Reflection;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Entities;
using EducationPortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    private readonly ICurrentTenant _tenant;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentTenant tenant) : base(options)
    {
        _tenant = tenant;
    }

    /// <summary>The security audit log. The application only ever adds to it (SEC-LOG-05).</summary>
    public DbSet<SecurityAuditEvent> SecurityAuditEvents => Set<SecurityAuditEvent>();

    public DbSet<School> Schools => Set<School>();

    public DbSet<BrandingImage> BrandingImages => Set<BrandingImage>();

    public DbSet<Country> Countries => Set<Country>();

    // Read by the query filters on every query, so each request sees its own tenant (ENG-EF-10).
    private int? CurrentSchoolId => _tenant.SchoolId;

    private bool IsMasterTenant => _tenant.IsMaster;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Tenant isolation (SEC-AUTHZ-03): a school sees only its own rows, the master tenant sees every school's. With
        // no resolved tenant, CurrentSchoolId is null and nothing matches.
        builder.Entity<BrandingImage>()
            .HasQueryFilter(i => IsMasterTenant || i.SchoolId == CurrentSchoolId);
    }
}

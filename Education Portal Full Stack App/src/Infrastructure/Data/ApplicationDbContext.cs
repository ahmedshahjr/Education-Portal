using System.Reflection;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Common;
using EducationPortal.Domain.Entities;
using EducationPortal.Infrastructure.Data.Lookups;
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

    public DbSet<SchoolRequest> SchoolRequests => Set<SchoolRequest>();

    public DbSet<PortalModule> Modules => Set<PortalModule>();

    public DbSet<SchoolModule> SchoolModules => Set<SchoolModule>();

    // Read by the query filters on every query, so each request sees its own tenant (ENG-EF-10).
    private long? CurrentSchoolId => _tenant.SchoolId;

    private bool IsMasterTenant => _tenant.IsMaster;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Enum columns hold numbers; a seeded lookup table per enum names them for reports (ENG-ENUM-04).
        EnumLookupTables.Configure(builder);

        // Tenant isolation (SEC-AUTHZ-03): a school sees only its own rows, the master tenant sees every school's. With
        // no resolved tenant, CurrentSchoolId is null and nothing matches.
        builder.Entity<BrandingImage>()
            .HasQueryFilter(i => IsMasterTenant || i.SchoolId == CurrentSchoolId);

        builder.Entity<SchoolModule>()
            .HasQueryFilter(m => IsMasterTenant || m.SchoolId == CurrentSchoolId);

        // Every entity's Guid PublicId is an alternate key (a unique constraint), the only identity the API exposes. The
        // long Id stays the primary key for joins and foreign keys (ENG-ID-01, ENG-ID-02).
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType) && entityType.BaseType is null)
            {
                builder.Entity(entityType.ClrType).HasAlternateKey(nameof(BaseEntity.PublicId));
            }
        }

        // Soft delete: every soft-deletable entity hides its deleted rows from every query, the master
        // tenant included. Rows stay in the table for audit.
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseSoftDeletableEntity).IsAssignableFrom(entityType.ClrType))
            {
                ApplySoftDeleteFilter(builder, entityType.ClrType);
            }
        }
    }

    private static void ApplySoftDeleteFilter(ModelBuilder builder, Type entityClrType)
    {
        var method = typeof(ApplicationDbContext)
            .GetMethod(nameof(SoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(entityClrType);
        method.Invoke(null, new object[] { builder });
    }

    private static void SoftDeleteFilter<TEntity>(ModelBuilder builder) where TEntity : BaseSoftDeletableEntity =>
        builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
}

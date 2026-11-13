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

    public DbSet<State> States => Set<State>();

    public DbSet<City> Cities => Set<City>();

    public DbSet<SchoolRequest> SchoolRequests => Set<SchoolRequest>();

    public DbSet<PortalModule> Modules => Set<PortalModule>();

    public DbSet<ModuleDependency> ModuleDependencies => Set<ModuleDependency>();

    public DbSet<Plan> Plans => Set<Plan>();

    public DbSet<PlanModule> PlanModules => Set<PlanModule>();

    public DbSet<PriceBook> PriceBooks => Set<PriceBook>();

    public DbSet<PriceBookItem> PriceBookItems => Set<PriceBookItem>();

    public DbSet<Branch> Branches => Set<Branch>();

    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    public DbSet<TenantModule> TenantModules => Set<TenantModule>();

    public DbSet<AddOnRequest> AddOnRequests => Set<AddOnRequest>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<SchoolRole> SchoolRoles => Set<SchoolRole>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // Read by the query filters on every query, so each request sees its own tenant (ENG-EF-10).
    private int? CurrentSchoolId => _tenant.SchoolId;

    private bool IsMasterTenant => _tenant.IsMaster;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.HasSequence<int>(Configurations.InvoiceConfiguration.NumberSequence);

        // Enum columns hold numbers; a seeded lookup table per enum names them for reports (ENG-ENUM-04).
        EnumLookupTables.Configure(builder);

        // Tenant isolation (SEC-AUTHZ-03): a school sees only its own rows, the master tenant sees every school's. With
        // no resolved tenant, CurrentSchoolId is null and nothing matches.
        builder.Entity<BrandingImage>()
            .HasQueryFilter(i => IsMasterTenant || i.SchoolId == CurrentSchoolId);

        builder.Entity<Branch>()
            .HasQueryFilter(b => IsMasterTenant || b.SchoolId == CurrentSchoolId);

        builder.Entity<Subscription>()
            .HasQueryFilter(s => IsMasterTenant || s.SchoolId == CurrentSchoolId);

        builder.Entity<TenantModule>()
            .HasQueryFilter(m => IsMasterTenant || m.SchoolId == CurrentSchoolId);

        builder.Entity<AddOnRequest>()
            .HasQueryFilter(r => IsMasterTenant || r.SchoolId == CurrentSchoolId);

        builder.Entity<Invoice>()
            .HasQueryFilter(i => IsMasterTenant || i.SchoolId == CurrentSchoolId);

        builder.Entity<InvoiceLine>()
            .HasQueryFilter(l => IsMasterTenant || l.SchoolId == CurrentSchoolId);

        builder.Entity<Payment>()
            .HasQueryFilter(p => IsMasterTenant || p.SchoolId == CurrentSchoolId);

        builder.Entity<SchoolRole>()
            .HasQueryFilter(r => IsMasterTenant || r.SchoolId == CurrentSchoolId);

        builder.Entity<RolePermission>()
            .HasQueryFilter(p => IsMasterTenant || p.SchoolId == CurrentSchoolId);

        // Every entity's Guid PublicId is an alternate key (a unique constraint), the only identity the API exposes. The
        // int Id stays the primary key for joins and foreign keys (ENG-ID-01, ENG-ID-02).
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

    public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (Database.CurrentTransaction is not null)
        {
            return await operation(cancellationToken);
        }

        // Aspire's Npgsql integration retries transient failures, so the whole transaction runs as one retriable unit
        // under that execution strategy. Disposing the transaction without a commit rolls it back, so any exception
        // undoes every save (fail closed).
        var strategy = Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async token =>
        {
            await using var transaction = await Database.BeginTransactionAsync(token);
            var result = await operation(token);
            await transaction.CommitAsync(token);

            return result;
        }, cancellationToken);
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

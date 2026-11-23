using System.Linq.Expressions;
using System.Reflection;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Common;
using EducationPortal.Domain.Entities;
using EducationPortal.Infrastructure.Data.Lookups;
using EducationPortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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

    /// <summary>Changes to commercial records with old and new values, written by <c>ChangeAuditInterceptor</c> (ADR-016). Append-only.</summary>
    public DbSet<ChangeAuditEntry> ChangeAuditEntries => Set<ChangeAuditEntry>();

    public DbSet<School> Schools => Set<School>();

    public DbSet<Tenant> Tenants => Set<Tenant>();

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

    public DbSet<Campus> Campuses => Set<Campus>();

    public DbSet<CampusPhone> CampusPhones => Set<CampusPhone>();

    public DbSet<UserDetails> UserDetails => Set<UserDetails>();

    public DbSet<ParentStudent> ParentStudents => Set<ParentStudent>();

    public DbSet<AccountRole> AccountRoles => Set<AccountRole>();

    public DbSet<UserCampus> UserCampuses => Set<UserCampus>();

    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();

    public DbSet<SchoolProfile> SchoolProfiles => Set<SchoolProfile>();

    public DbSet<SchoolCurriculum> SchoolCurricula => Set<SchoolCurriculum>();

    public DbSet<SchoolFacility> SchoolFacilities => Set<SchoolFacility>();

    public DbSet<ClassOffering> ClassOfferings => Set<ClassOffering>();

    public DbSet<SchoolPhoto> SchoolPhotos => Set<SchoolPhoto>();

    public DbSet<AdmissionEnquiry> AdmissionEnquiries => Set<AdmissionEnquiry>();

    public DbSet<EnquiryFollowUp> EnquiryFollowUps => Set<EnquiryFollowUp>();

    public DbSet<CampusVisit> CampusVisits => Set<CampusVisit>();

    public DbSet<SchoolReview> SchoolReviews => Set<SchoolReview>();

    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    public DbSet<TenantModule> TenantModules => Set<TenantModule>();

    public DbSet<AddOnRequest> AddOnRequests => Set<AddOnRequest>();

    public DbSet<RenewalReminder> RenewalReminders => Set<RenewalReminder>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<SchoolRole> SchoolRoles => Set<SchoolRole>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();

    public DbSet<Term> Terms => Set<Term>();

    public DbSet<Shift> Shifts => Set<Shift>();

    public DbSet<Holiday> Holidays => Set<Holiday>();

    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Enrolment> Enrolments => Set<Enrolment>();

    public DbSet<Assessment> Assessments => Set<Assessment>();

    public DbSet<Score> Scores => Set<Score>();

    public DbSet<TimetableSlot> TimetableSlots => Set<TimetableSlot>();

    public DbSet<GradingScale> GradingScales => Set<GradingScale>();

    public DbSet<GradingBand> GradingBands => Set<GradingBand>();

    public DbSet<SchoolEvent> SchoolEvents => Set<SchoolEvent>();

    public DbSet<Notification> Notifications => Set<Notification>();


    // Read by the query filters on every query, so each request sees its own school and tenant (ENG-EF-10).
    private int? CurrentSchoolId => _tenant.SchoolId;

    private int? CurrentTenantId => _tenant.TenantId;

    private bool BypassTenantFilters => _tenant.BypassesTenantFilters;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.HasSequence<int>(Configurations.InvoiceConfiguration.NumberSequence, DatabaseSchemas.Billing);

        // Enum columns hold numbers; a seeded lookup table per enum names them for reports (ENG-ENUM-04).
        EnumLookupTables.Configure(builder);

        // Last, so every entity type of the model, the lookup tables included, is in its feature schema.
        builder.ApplyFeatureSchemas();

        // Tenant isolation (SEC-AUTHZ-03, ADR-026). Every ITenantOwned row has a TenantId column that the database fills from
        // its school (a trigger, so the application cannot choose it). The row is visible only to its own school inside its
        // own tenant; a master session sees every row. With no resolved school, nothing matches.
        // Every entity that implements ITenantOwned gets the same column, foreign key, index and filter here, so a new
        // school-owned entity cannot miss them. Tests (TenantIsolationModelTests) fail when one is missing.
        foreach (var entityType in builder.Model.GetEntityTypes().Where(t => typeof(ITenantOwned).IsAssignableFrom(t.ClrType)).ToList())
        {
            var entity = builder.Entity(entityType.ClrType);
            entity.Property<int>(ITenantOwned.TenantColumn).IsRequired();
            entity.HasOne(typeof(Tenant)).WithMany().HasForeignKey(ITenantOwned.TenantColumn).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(ITenantOwned.TenantColumn, nameof(ITenantOwned.SchoolId));

            entityType.SetQueryFilter(TenantFilter(entityType.ClrType));
        }

        // The audit trail has no school for entries the system writes, so its SchoolId is nullable and it is not ITenantOwned.
        builder.Entity<ChangeAuditEntry>()
            .HasQueryFilter(e => BypassTenantFilters || e.SchoolId == CurrentSchoolId);

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
        // tenant included. Rows stay in the table for audit. Added to the tenant filter, not in place of it.
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseSoftDeletableEntity).IsAssignableFrom(entityType.ClrType))
            {
                ApplySoftDeleteFilter(entityType);
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

    /// <summary>
    /// The tenant filter of an <see cref="ITenantOwned"/> entity: a master session sees every row; any other request sees the
    /// rows of its own school, and only while the row's tenant is the school's tenant (ADR-026).
    /// </summary>
    private LambdaExpression TenantFilter(Type entityClrType)
    {
        var entity = Expression.Parameter(entityClrType, "e");
        var self = Expression.Constant(this);
        var bypass = Expression.Property(self, nameof(BypassTenantFilters));
        var rowSchool = Expression.Convert(Expression.Property(entity, nameof(ITenantOwned.SchoolId)), typeof(int?));
        var sameSchool = Expression.Equal(rowSchool, Expression.Property(self, nameof(CurrentSchoolId)));
        // EF.Property reads the shadow column, which has no CLR property on the entity (ADR-026).
        var tenantColumn = Expression.Call(
            typeof(EF).GetMethod(nameof(EF.Property))!.MakeGenericMethod(typeof(int)),
            entity,
            Expression.Constant(ITenantOwned.TenantColumn));
        var rowTenant = Expression.Convert(tenantColumn, typeof(int?));
        var sameTenant = Expression.Equal(rowTenant, Expression.Property(self, nameof(CurrentTenantId)));
        return Expression.Lambda(Expression.OrElse(bypass, Expression.AndAlso(sameTenant, sameSchool)), entity);
    }

    /// <summary>
    /// Adds "not deleted" to the query filter the entity type already has. EF Core keeps one filter per entity type, so a
    /// new filter would replace the tenant filter (SEC-AUTHZ-03) that the school's rows are scoped by.
    /// </summary>
    private static void ApplySoftDeleteFilter(IMutableEntityType entityType)
    {
        var entity = Expression.Parameter(entityType.ClrType, "e");
        Expression notDeleted = Expression.Not(Expression.Property(entity, nameof(BaseSoftDeletableEntity.IsDeleted)));

        // The tenant filter is the only declared filter of these entity types, so it is combined with "not deleted".
        foreach (var filter in entityType.GetDeclaredQueryFilters())
        {
            var tenantFilter = filter.Expression ?? throw new InvalidOperationException($"The query filter of {entityType.ClrType.Name} has no expression.");
            var existingBody = new ParameterReplacer(tenantFilter.Parameters[0], entity).Visit(tenantFilter.Body);
            notDeleted = Expression.AndAlso(existingBody, notDeleted);
        }

        entityType.SetQueryFilter(Expression.Lambda(notDeleted, entity));
    }

    /// <summary>Points a filter's parameter at the parameter of the combined filter.</summary>
    private sealed class ParameterReplacer(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : base.VisitParameter(node);
    }
}

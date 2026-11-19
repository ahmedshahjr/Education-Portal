using EducationPortal.Domain.Entities;

namespace EducationPortal.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    /// <summary>The tenants. Handlers always scope reads to the current tenant or to a master-only use case.</summary>
    DbSet<School> Schools { get; }

    /// <summary>Logos and favicons. Covered by the tenant query filter (SEC-AUTHZ-03).</summary>
    DbSet<BrandingImage> BrandingImages { get; }

    /// <summary>Public requests from schools that want to join. Only the master tenant decides them.</summary>
    DbSet<SchoolRequest> SchoolRequests { get; }

    /// <summary>Countries and their regional settings. Platform reference data that only the master tenant changes.</summary>
    DbSet<Country> Countries { get; }

    /// <summary>States and provinces of each country, for address dropdowns. Platform reference data, not owned by a school.</summary>
    DbSet<State> States { get; }

    /// <summary>Cities of each state, for address dropdowns. Platform reference data, not owned by a school.</summary>
    DbSet<City> Cities { get; }

    /// <summary>The module catalogue: what the platform sells. Platform reference data that only the master tenant changes.</summary>
    DbSet<PortalModule> Modules { get; }

    /// <summary>Which modules require which. Platform reference data that only the master tenant changes.</summary>
    DbSet<ModuleDependency> ModuleDependencies { get; }

    /// <summary>The master tenant's per-school module switches. Covered by the tenant query filter (SEC-AUTHZ-03).</summary>
    /// <summary>The plans the master tenant sells. Platform reference data that only the master tenant changes.</summary>
    DbSet<Plan> Plans { get; }

    /// <summary>The paid modules in each plan.</summary>
    DbSet<PlanModule> PlanModules { get; }

    /// <summary>One price book per country, with its currency. Platform reference data that only the master tenant changes.</summary>
    DbSet<PriceBook> PriceBooks { get; }

    /// <summary>The prices of plans and add-on modules in each price book.</summary>
    DbSet<PriceBookItem> PriceBookItems { get; }

    DbSet<Campus> Campuses { get; }
    /// <summary>The phone numbers of each campus, in order.</summary>
    DbSet<CampusPhone> CampusPhones { get; }
    /// <summary>The people of each school (principals, teachers, staff). Owned by the school and filtered by it.</summary>
    DbSet<UserDetails> UserDetails { get; }

    DbSet<ParentStudent> ParentStudents { get; }

    DbSet<ChangeAuditEntry> ChangeAuditEntries { get; }

    DbSet<AccountRole> AccountRoles { get; }

    DbSet<Course> Courses { get; }

    DbSet<Enrolment> Enrolments { get; }

    DbSet<Assessment> Assessments { get; }

    DbSet<Score> Scores { get; }

    DbSet<TimetableSlot> TimetableSlots { get; }
    /// <summary>The admission form of each student, one per student person. Owned by the school and filtered by it.</summary>
    DbSet<StudentProfile> StudentProfiles { get; }
    // The public page of a school and its admission enquiries. Owned by the school and filtered by it.
    DbSet<SchoolProfile> SchoolProfiles { get; }
    DbSet<SchoolCurriculum> SchoolCurricula { get; }
    DbSet<SchoolFacility> SchoolFacilities { get; }
    DbSet<ClassOffering> ClassOfferings { get; }
    DbSet<SchoolPhoto> SchoolPhotos { get; }
    DbSet<AdmissionEnquiry> AdmissionEnquiries { get; }
    DbSet<EnquiryFollowUp> EnquiryFollowUps { get; }
    DbSet<CampusVisit> CampusVisits { get; }
    DbSet<SchoolReview> SchoolReviews { get; }


    DbSet<Subscription> Subscriptions { get; }

    DbSet<TenantModule> TenantModules { get; }

    DbSet<AddOnRequest> AddOnRequests { get; }

    /// <summary>Renewal reminders queued by the daily lifecycle job. Covered by the tenant query filter (SEC-AUTHZ-03).</summary>
    DbSet<RenewalReminder> RenewalReminders { get; }

    DbSet<Invoice> Invoices { get; }

    DbSet<InvoiceLine> InvoiceLines { get; }

    DbSet<Payment> Payments { get; }

    DbSet<SchoolRole> SchoolRoles { get; }

    DbSet<RolePermission> RolePermissions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Runs <paramref name="operation"/> in one database transaction, so every save inside it, Identity's included, is
    /// committed together or not at all. An exception rolls everything back. Inside a running transaction it just runs.
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken);
}

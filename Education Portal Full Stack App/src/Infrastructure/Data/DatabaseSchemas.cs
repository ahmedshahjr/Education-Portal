using EducationPortal.Domain.Entities;
using EducationPortal.Infrastructure.Data.Lookups;
using EducationPortal.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.Infrastructure.Data;

/// <summary>
/// One PostgreSQL schema per feature area, so the tables of a feature are grouped together in the database. Every entity
/// type must have a schema here: <see cref="ApplyFeatureSchemas"/> throws for an unmapped type, so a new entity cannot
/// silently land in <c>public</c>.
/// </summary>
public static class DatabaseSchemas
{
    public const string Identity = "identity";
    public const string Access = "access";
    public const string Schools = "schools";
    public const string Modules = "modules";
    public const string Billing = "billing";
    public const string Audit = "audit";
    public const string Lookups = "lookups";
    public const string Admissions = "admissions";
    public const string Academics = "academics";

    /// <summary>Every feature schema, for tools that reset or inspect the database (Respawn in the functional tests).</summary>
    public static readonly string[] All = [Identity, Access, Schools, Modules, Billing, Audit, Lookups, Admissions, Academics];

    private static readonly Dictionary<Type, string> BySchool = new()
    {
        [typeof(SchoolRole)] = Access,
        [typeof(RolePermission)] = Access,
        [typeof(AccountRole)] = Access,

        [typeof(School)] = Schools,
        [typeof(Campus)] = Schools,
        [typeof(CampusPhone)] = Schools,
        [typeof(UserDetails)] = Schools,
        [typeof(StudentProfile)] = Schools,
        [typeof(ParentStudent)] = Schools,
        [typeof(SchoolProfile)] = Schools,
        [typeof(SchoolCurriculum)] = Schools,
        [typeof(SchoolFacility)] = Schools,
        [typeof(ClassOffering)] = Schools,
        [typeof(SchoolPhoto)] = Schools,
        [typeof(SchoolReview)] = Schools,
        [typeof(AdmissionEnquiry)] = Admissions,
        [typeof(EnquiryFollowUp)] = Admissions,
        [typeof(CampusVisit)] = Admissions,

        [typeof(BrandingImage)] = Schools,
        [typeof(SchoolRequest)] = Schools,
        [typeof(Country)] = Schools,
        [typeof(State)] = Schools,
        [typeof(City)] = Schools,

        [typeof(PortalModule)] = Modules,
        [typeof(ModuleDependency)] = Modules,
        [typeof(TenantModule)] = Modules,

        [typeof(Plan)] = Billing,
        [typeof(PlanModule)] = Billing,
        [typeof(PriceBook)] = Billing,
        [typeof(PriceBookItem)] = Billing,
        [typeof(Subscription)] = Billing,
        [typeof(AddOnRequest)] = Billing,
        [typeof(RenewalReminder)] = Billing,
        [typeof(Invoice)] = Billing,
        [typeof(InvoiceLine)] = Billing,
        [typeof(Payment)] = Billing,

        [typeof(Course)] = Academics,
        [typeof(Enrolment)] = Academics,
        [typeof(Assessment)] = Academics,
        [typeof(Score)] = Academics,
        [typeof(TimetableSlot)] = Academics,
        [typeof(GradingScale)] = Academics,
        [typeof(GradingBand)] = Academics,

        [typeof(SecurityAuditEvent)] = Audit,
        [typeof(ChangeAuditEntry)] = Audit,
    };

    /// <summary>Puts every entity type of the model into its feature schema. Call after the entity configurations are applied.</summary>
    public static void ApplyFeatureSchemas(this ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            // An owned type or a complex type (a value object) is stored in its owner's table, so it takes the owner's
            // schema. A complex type has no key of its own; every table-backed entity has one.
            if (entityType.IsOwned() || entityType.FindPrimaryKey() is null)
            {
                continue;
            }

            entityType.SetSchema(SchemaFor(entityType.ClrType, entityType.Name));
        }
    }

    private static string SchemaFor(Type clrType, string name)
    {
        if (clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(EnumLookup<>))
        {
            return Lookups;
        }

        if (clrType == typeof(ApplicationUser) || clrType.Namespace?.StartsWith("Microsoft.AspNetCore.Identity", StringComparison.Ordinal) == true)
        {
            return Identity;
        }

        if (BySchool.TryGetValue(clrType, out var schema))
        {
            return schema;
        }

        throw new InvalidOperationException($"Entity '{name}' has no feature schema. Add it to DatabaseSchemas.");
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EducationPortal.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class initialmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "academics");

            migrationBuilder.EnsureSchema(
                name: "lookups");

            migrationBuilder.EnsureSchema(
                name: "access");

            migrationBuilder.EnsureSchema(
                name: "billing");

            migrationBuilder.EnsureSchema(
                name: "admissions");

            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.EnsureSchema(
                name: "schools");

            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.EnsureSchema(
                name: "modules");

            migrationBuilder.EnsureSchema(
                name: "communication");

            migrationBuilder.CreateSequence<int>(
                name: "InvoiceNumbers",
                schema: "billing");

            migrationBuilder.CreateTable(
                name: "AcademicYearStatuses",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicYearStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AddOnRequestStatuses",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddOnRequestStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdmissionStatuses",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdmissionStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BillingCycles",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingCycles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BrandingImageKinds",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandingImageKinds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CalendarEventAudiences",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEventAudiences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CalendarEventTypes",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEventTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampusKinds",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampusKinds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChangeAuditActions",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeAuditActions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClassLevels",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurriculumKinds",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumKinds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataRegions",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataRegions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DaysOfWeek",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DaysOfWeek", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnquiryChannels",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryChannels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnquiryStatuses",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventAudiences",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventAudiences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FacilityKinds",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacilityKinds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FollowUpKinds",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowUpKinds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genders",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GradingScales",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingScales", x => x.Id);
                    table.UniqueConstraint("AK_GradingScales_PublicId", x => x.PublicId);
                });

            migrationBuilder.CreateTable(
                name: "HolidayAudiences",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HolidayAudiences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HolidayTypes",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HolidayTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceStatuses",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModuleCodes",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhotoKinds",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhotoKinds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                    table.UniqueConstraint("AK_Plans_PublicId", x => x.PublicId);
                });

            migrationBuilder.CreateTable(
                name: "PriceBooks",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CountryCode = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceBooks", x => x.Id);
                    table.UniqueConstraint("AK_PriceBooks_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_PriceBooks_TaxRateRange", "\"TaxRate\" >= 0 AND \"TaxRate\" <= 100");
                });

            migrationBuilder.CreateTable(
                name: "ReviewerRelations",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewerRelations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReviewStatuses",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchoolGenderPolicies",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolGenderPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchoolRequestStatuses",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolRequestStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityAuditEventTypes",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityAuditEventTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityAuditOutcomes",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityAuditOutcomes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StateTypes",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionStatuses",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantModuleStatuses",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantModuleStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsMaster = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                    table.UniqueConstraint("AK_Tenants_PublicId", x => x.PublicId);
                });

            migrationBuilder.CreateTable(
                name: "UserTypes",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisitStatuses",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Weekdays",
                schema: "lookups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Weekdays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "identity",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChangeAuditEntries",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActorUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    SchoolId = table.Column<int>(type: "integer", nullable: true),
                    EntityType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    Changes = table.Column<string>(type: "jsonb", nullable: false),
                    TraceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeAuditEntries", x => x.Id);
                    table.UniqueConstraint("AK_ChangeAuditEntries_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_ChangeAuditEntries_ChangeAuditActions_Action",
                        column: x => x.Action,
                        principalSchema: "lookups",
                        principalTable: "ChangeAuditActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Iso3 = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: true),
                    PhoneCode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    NativeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Languages = table.Column<string[]>(type: "character varying(2)[]", nullable: false),
                    DefaultLanguage = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    TimeZoneId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    CurrencyDecimals = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    PhonePattern = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WeekStartDay = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    WeekendDays = table.Column<int[]>(type: "integer[]", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                    table.UniqueConstraint("AK_Countries_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_Countries_CurrencyDecimals", "\"CurrencyDecimals\" BETWEEN 0 AND 4");
                    table.ForeignKey(
                        name: "FK_Countries_DaysOfWeek_WeekStartDay",
                        column: x => x.WeekStartDay,
                        principalSchema: "lookups",
                        principalTable: "DaysOfWeek",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SchoolEvents",
                schema: "communication",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    StartsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Audience = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolEvents", x => x.Id);
                    table.UniqueConstraint("AK_SchoolEvents_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_SchoolEvents_EventAudiences_Audience",
                        column: x => x.Audience,
                        principalSchema: "lookups",
                        principalTable: "EventAudiences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GradingBands",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    GradingScaleId = table.Column<int>(type: "integer", nullable: false),
                    MinPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    MaxPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Letter = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Points = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingBands", x => x.Id);
                    table.UniqueConstraint("AK_GradingBands_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_GradingBands_GradingScales_GradingScaleId",
                        column: x => x.GradingScaleId,
                        principalSchema: "academics",
                        principalTable: "GradingScales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                schema: "modules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IsCore = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsDefaultEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                    table.UniqueConstraint("AK_Modules_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_Modules_ModuleCodes_Code",
                        column: x => x.Code,
                        principalSchema: "lookups",
                        principalTable: "ModuleCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SchoolRequests",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolRequests", x => x.Id);
                    table.UniqueConstraint("AK_SchoolRequests_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_SchoolRequests_SchoolRequestStatuses_Status",
                        column: x => x.Status,
                        principalSchema: "lookups",
                        principalTable: "SchoolRequestStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SecurityAuditEvents",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    Outcome = table.Column<int>(type: "integer", nullable: false),
                    ActorUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ActorSchoolId = table.Column<int>(type: "integer", nullable: true),
                    ActorCampusId = table.Column<int>(type: "integer", nullable: true),
                    TargetType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    TargetId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    SourceIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    TraceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityAuditEvents", x => x.Id);
                    table.UniqueConstraint("AK_SecurityAuditEvents_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_SecurityAuditEvents_SecurityAuditEventTypes_EventType",
                        column: x => x.EventType,
                        principalSchema: "lookups",
                        principalTable: "SecurityAuditEventTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SecurityAuditEvents_SecurityAuditOutcomes_Outcome",
                        column: x => x.Outcome,
                        principalSchema: "lookups",
                        principalTable: "SecurityAuditOutcomes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "States",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CountryId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NativeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsoCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsCustom = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States", x => x.Id);
                    table.UniqueConstraint("AK_States_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_States_Countries_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "schools",
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_States_StateTypes_Type",
                        column: x => x.Type,
                        principalSchema: "lookups",
                        principalTable: "StateTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "communication",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    RecipientAccountId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    SchoolEventId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.UniqueConstraint("AK_Notifications_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_Notifications_SchoolEvents_SchoolEventId",
                        column: x => x.SchoolEventId,
                        principalSchema: "communication",
                        principalTable: "SchoolEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModuleDependencies",
                schema: "modules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ModuleId = table.Column<int>(type: "integer", nullable: false),
                    RequiresModuleId = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleDependencies", x => x.Id);
                    table.UniqueConstraint("AK_ModuleDependencies_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_ModuleDependencies_NotSelf", "\"ModuleId\" <> \"RequiresModuleId\"");
                    table.ForeignKey(
                        name: "FK_ModuleDependencies_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "modules",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ModuleDependencies_Modules_RequiresModuleId",
                        column: x => x.RequiresModuleId,
                        principalSchema: "modules",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlanModules",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanId = table.Column<int>(type: "integer", nullable: false),
                    ModuleId = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanModules", x => x.Id);
                    table.UniqueConstraint("AK_PlanModules_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_PlanModules_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "modules",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanModules_Plans_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "billing",
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PriceBookItems",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PriceBookId = table.Column<int>(type: "integer", nullable: false),
                    PlanId = table.Column<int>(type: "integer", nullable: true),
                    ModuleId = table.Column<int>(type: "integer", nullable: true),
                    BillingCycle = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceBookItems", x => x.Id);
                    table.UniqueConstraint("AK_PriceBookItems_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_PriceBookItems_PlanOrModule", "(\"PlanId\" IS NULL) <> (\"ModuleId\" IS NULL)");
                    table.CheckConstraint("CK_PriceBookItems_PriceNotNegative", "\"Price\" >= 0");
                    table.ForeignKey(
                        name: "FK_PriceBookItems_BillingCycles_BillingCycle",
                        column: x => x.BillingCycle,
                        principalSchema: "lookups",
                        principalTable: "BillingCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PriceBookItems_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "modules",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PriceBookItems_Plans_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "billing",
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PriceBookItems_PriceBooks_PriceBookId",
                        column: x => x.PriceBookId,
                        principalSchema: "billing",
                        principalTable: "PriceBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StateId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NativeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric(10,8)", precision: 10, scale: 8, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(11,8)", precision: 11, scale: 8, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsCustom = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                    table.UniqueConstraint("AK_Cities_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_Cities_States_StateId",
                        column: x => x.StateId,
                        principalSchema: "schools",
                        principalTable: "States",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Schools",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Domain = table.Column<string>(type: "character varying(253)", maxLength: 253, nullable: true),
                    IsMaster = table.Column<bool>(type: "boolean", nullable: false),
                    AccentColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    SidebarColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    CountryId = table.Column<int>(type: "integer", nullable: true),
                    DataRegion = table.Column<int>(type: "integer", nullable: true),
                    TaxRateOverride = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    CampusCodePrefixEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CampusCodePrefix = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    LocalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RegistrationAuthority = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TaxNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CityId = table.Column<int>(type: "integer", nullable: true),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    Website = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    AcademicYearStartMonth = table.Column<int>(type: "integer", nullable: false, defaultValue: 8),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.Id);
                    table.UniqueConstraint("AK_Schools_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_Schools_AcademicYearStartMonth", "\"AcademicYearStartMonth\" BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_Schools_TaxRateOverrideRange", "\"TaxRateOverride\" IS NULL OR (\"TaxRateOverride\" >= 0 AND \"TaxRateOverride\" <= 100)");
                    table.ForeignKey(
                        name: "FK_Schools_Cities_CityId",
                        column: x => x.CityId,
                        principalSchema: "schools",
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Schools_Countries_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "schools",
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Schools_DataRegions_DataRegion",
                        column: x => x.DataRegion,
                        principalSchema: "lookups",
                        principalTable: "DataRegions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Schools_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "schools",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AcademicYears",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicYears", x => x.Id);
                    table.UniqueConstraint("AK_AcademicYears_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_AcademicYears_Dates", "\"EndDate\" >= \"StartDate\"");
                    table.ForeignKey(
                        name: "FK_AcademicYears_AcademicYearStatuses_Status",
                        column: x => x.Status,
                        principalSchema: "lookups",
                        principalTable: "AcademicYearStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AcademicYears_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AddOnRequests",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    ModuleId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DecidedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddOnRequests", x => x.Id);
                    table.UniqueConstraint("AK_AddOnRequests_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_AddOnRequests_AddOnRequestStatuses_Status",
                        column: x => x.Status,
                        principalSchema: "lookups",
                        principalTable: "AddOnRequestStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AddOnRequests_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "modules",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AddOnRequests_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    PreferredLanguage = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    MustChangePassword = table.Column<bool>(type: "boolean", nullable: false),
                    UserDetailsId = table.Column<int>(type: "integer", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BrandingImages",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Content = table.Column<byte[]>(type: "bytea", nullable: false),
                    Version = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandingImages", x => x.Id);
                    table.UniqueConstraint("AK_BrandingImages_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_BrandingImages_BrandingImageKinds_Kind",
                        column: x => x.Kind,
                        principalSchema: "lookups",
                        principalTable: "BrandingImageKinds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BrandingImages_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SchoolCurricula",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolCurricula", x => x.Id);
                    table.UniqueConstraint("AK_SchoolCurricula_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_SchoolCurricula_CurriculumKinds_Kind",
                        column: x => x.Kind,
                        principalSchema: "lookups",
                        principalTable: "CurriculumKinds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolCurricula_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchoolFacilities",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolFacilities", x => x.Id);
                    table.UniqueConstraint("AK_SchoolFacilities_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_SchoolFacilities_FacilityKinds_Kind",
                        column: x => x.Kind,
                        principalSchema: "lookups",
                        principalTable: "FacilityKinds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolFacilities_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchoolProfiles",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    Tagline = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    About = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    EstablishedYear = table.Column<int>(type: "integer", nullable: true),
                    GenderPolicy = table.Column<int>(type: "integer", nullable: false),
                    VisitingHours = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VirtualTourUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolProfiles", x => x.Id);
                    table.UniqueConstraint("AK_SchoolProfiles_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_SchoolProfiles_SchoolGenderPolicies_GenderPolicy",
                        column: x => x.GenderPolicy,
                        principalSchema: "lookups",
                        principalTable: "SchoolGenderPolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolProfiles_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchoolRoles",
                schema: "access",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolRoles", x => x.Id);
                    table.UniqueConstraint("AK_SchoolRoles_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_SchoolRoles_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    PlanId = table.Column<int>(type: "integer", nullable: true),
                    PriceBookId = table.Column<int>(type: "integer", nullable: false),
                    CurrencyCode = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    BillingCycle = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TrialEndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PastDueSince = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.UniqueConstraint("AK_Subscriptions_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_Subscriptions_AmountNotNegative", "\"Amount\" >= 0");
                    table.ForeignKey(
                        name: "FK_Subscriptions_BillingCycles_BillingCycle",
                        column: x => x.BillingCycle,
                        principalSchema: "lookups",
                        principalTable: "BillingCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Plans_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "billing",
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscriptions_PriceBooks_PriceBookId",
                        column: x => x.PriceBookId,
                        principalSchema: "billing",
                        principalTable: "PriceBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscriptions_SubscriptionStatuses_Status",
                        column: x => x.Status,
                        principalSchema: "lookups",
                        principalTable: "SubscriptionStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserDetails",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserType = table.Column<int>(type: "integer", nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDetails", x => x.Id);
                    table.UniqueConstraint("AK_UserDetails_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_UserDetails_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDetails_UserTypes_UserType",
                        column: x => x.UserType,
                        principalSchema: "lookups",
                        principalTable: "UserTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Terms",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    AcademicYearId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terms", x => x.Id);
                    table.UniqueConstraint("AK_Terms_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_Terms_Dates", "\"EndDate\" >= \"StartDate\"");
                    table.CheckConstraint("CK_Terms_Sequence", "\"Sequence\" BETWEEN 1 AND 12");
                    table.ForeignKey(
                        name: "FK_Terms_AcademicYears_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalSchema: "academics",
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Terms_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "identity",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "identity",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "identity",
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "identity",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountRoles",
                schema: "access",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    SchoolRoleId = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountRoles", x => x.Id);
                    table.UniqueConstraint("AK_AccountRoles_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_AccountRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountRoles_SchoolRoles_SchoolRoleId",
                        column: x => x.SchoolRoleId,
                        principalSchema: "access",
                        principalTable: "SchoolRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    SubscriptionId = table.Column<int>(type: "integer", nullable: false),
                    Number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValueSql: "'INV-' || lpad(nextval('\"billing\".\"InvoiceNumbers\"')::text, 6, '0')"),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrencyCode = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IssueDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PeriodStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PaidDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OverdueSince = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.UniqueConstraint("AK_Invoices_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_Invoices_AmountsNotNegative", "\"Subtotal\" >= 0 AND \"TaxAmount\" >= 0 AND \"Total\" >= 0");
                    table.CheckConstraint("CK_Invoices_PeriodOrder", "\"PeriodEnd\" > \"PeriodStart\"");
                    table.ForeignKey(
                        name: "FK_Invoices_InvoiceStatuses_Status",
                        column: x => x.Status,
                        principalSchema: "lookups",
                        principalTable: "InvoiceStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalSchema: "billing",
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RenewalReminders",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    SubscriptionId = table.Column<int>(type: "integer", nullable: false),
                    PeriodEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DaysBefore = table.Column<int>(type: "integer", nullable: false),
                    QueuedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenewalReminders", x => x.Id);
                    table.UniqueConstraint("AK_RenewalReminders_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_RenewalReminders_DaysBefore", "\"DaysBefore\" BETWEEN 1 AND 365");
                    table.ForeignKey(
                        name: "FK_RenewalReminders_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RenewalReminders_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalSchema: "billing",
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Campuses",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    StateId = table.Column<int>(type: "integer", nullable: true),
                    CityId = table.Column<int>(type: "integer", nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Website = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    SchoolNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ShortName = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    PrincipalUserDetailsId = table.Column<int>(type: "integer", nullable: true),
                    LocalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GenderPolicy = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campuses", x => x.Id);
                    table.UniqueConstraint("AK_Campuses_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_Campuses_CityNeedsState", "\"CityId\" IS NULL OR \"StateId\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_Campuses_CampusKinds_Kind",
                        column: x => x.Kind,
                        principalSchema: "lookups",
                        principalTable: "CampusKinds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Campuses_Cities_CityId",
                        column: x => x.CityId,
                        principalSchema: "schools",
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Campuses_SchoolGenderPolicies_GenderPolicy",
                        column: x => x.GenderPolicy,
                        principalSchema: "lookups",
                        principalTable: "SchoolGenderPolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Campuses_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Campuses_States_StateId",
                        column: x => x.StateId,
                        principalSchema: "schools",
                        principalTable: "States",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Campuses_UserDetails_PrincipalUserDetailsId",
                        column: x => x.PrincipalUserDetailsId,
                        principalSchema: "schools",
                        principalTable: "UserDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParentStudents",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    ParentUserDetailsId = table.Column<int>(type: "integer", nullable: false),
                    StudentUserDetailsId = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentStudents", x => x.Id);
                    table.UniqueConstraint("AK_ParentStudents_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_ParentStudents_UserDetails_ParentUserDetailsId",
                        column: x => x.ParentUserDetailsId,
                        principalSchema: "schools",
                        principalTable: "UserDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParentStudents_UserDetails_StudentUserDetailsId",
                        column: x => x.StudentUserDetailsId,
                        principalSchema: "schools",
                        principalTable: "UserDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentProfiles",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    UserDetailsId = table.Column<int>(type: "integer", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    FatherName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nationality = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ClassApplied = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PreviousSchool = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GuardianPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AdmissionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProfiles", x => x.Id);
                    table.UniqueConstraint("AK_StudentProfiles_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_Genders_Gender",
                        column: x => x.Gender,
                        principalSchema: "lookups",
                        principalTable: "Genders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_UserDetails_UserDetailsId",
                        column: x => x.UserDetailsId,
                        principalSchema: "schools",
                        principalTable: "UserDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLines",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    InvoiceId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLines", x => x.Id);
                    table.UniqueConstraint("AK_InvoiceLines_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "billing",
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                schema: "billing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    InvoiceId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReceivedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.UniqueConstraint("AK_Payments_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_Payments_AmountPositive", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_Payments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "billing",
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_PaymentMethods_Method",
                        column: x => x.Method,
                        principalSchema: "lookups",
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdmissionEnquiries",
                schema: "admissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    CampusId = table.Column<int>(type: "integer", nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: true),
                    Channel = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ChildName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ConsentGivenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AssignedToUserDetailsId = table.Column<int>(type: "integer", nullable: true),
                    ContactedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdmissionEnquiries", x => x.Id);
                    table.UniqueConstraint("AK_AdmissionEnquiries_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_AdmissionEnquiries_OnlineNeedsConsent", "\"Channel\" <> 1 OR \"ConsentGivenAt\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_AdmissionEnquiries_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdmissionEnquiries_ClassLevels_Level",
                        column: x => x.Level,
                        principalSchema: "lookups",
                        principalTable: "ClassLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdmissionEnquiries_EnquiryChannels_Channel",
                        column: x => x.Channel,
                        principalSchema: "lookups",
                        principalTable: "EnquiryChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdmissionEnquiries_EnquiryStatuses_Status",
                        column: x => x.Status,
                        principalSchema: "lookups",
                        principalTable: "EnquiryStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdmissionEnquiries_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdmissionEnquiries_UserDetails_AssignedToUserDetailsId",
                        column: x => x.AssignedToUserDetailsId,
                        principalSchema: "schools",
                        principalTable: "UserDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CalendarEvents",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    CampusId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EventDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Audience = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEvents", x => x.Id);
                    table.UniqueConstraint("AK_CalendarEvents_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_CalendarEvents_EndNeedsStart", "\"EndTime\" IS NULL OR \"StartTime\" IS NOT NULL");
                    table.CheckConstraint("CK_CalendarEvents_Times", "\"EndTime\" IS NULL OR \"EndTime\" > \"StartTime\"");
                    table.ForeignKey(
                        name: "FK_CalendarEvents_CalendarEventAudiences_Audience",
                        column: x => x.Audience,
                        principalSchema: "lookups",
                        principalTable: "CalendarEventAudiences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalendarEvents_CalendarEventTypes_Type",
                        column: x => x.Type,
                        principalSchema: "lookups",
                        principalTable: "CalendarEventTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalendarEvents_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalendarEvents_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampusPhones",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    CampusId = table.Column<int>(type: "integer", nullable: false),
                    Number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampusPhones", x => x.Id);
                    table.UniqueConstraint("AK_CampusPhones_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_CampusPhones_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampusPhones_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassOfferings",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    CampusId = table.Column<int>(type: "integer", nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    AcademicYear = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    MonthlyFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AdmissionFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CurrencyCode = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    SeatsTotal = table.Column<int>(type: "integer", nullable: false),
                    SeatsAvailable = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ShowFee = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassOfferings", x => x.Id);
                    table.UniqueConstraint("AK_ClassOfferings_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_ClassOfferings_FeesNotNegative", "\"MonthlyFee\" >= 0 AND (\"AdmissionFee\" IS NULL OR \"AdmissionFee\" >= 0)");
                    table.CheckConstraint("CK_ClassOfferings_Seats", "\"SeatsTotal\" >= 0 AND \"SeatsAvailable\" >= 0 AND \"SeatsAvailable\" <= \"SeatsTotal\"");
                    table.ForeignKey(
                        name: "FK_ClassOfferings_AdmissionStatuses_Status",
                        column: x => x.Status,
                        principalSchema: "lookups",
                        principalTable: "AdmissionStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassOfferings_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassOfferings_ClassLevels_Level",
                        column: x => x.Level,
                        principalSchema: "lookups",
                        principalTable: "ClassLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassOfferings_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Holidays",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    CampusId = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    AppliesTo = table.Column<int>(type: "integer", nullable: false),
                    IsTentative = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.Id);
                    table.UniqueConstraint("AK_Holidays_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_Holidays_Dates", "\"EndDate\" >= \"StartDate\"");
                    table.ForeignKey(
                        name: "FK_Holidays_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Holidays_HolidayAudiences_AppliesTo",
                        column: x => x.AppliesTo,
                        principalSchema: "lookups",
                        principalTable: "HolidayAudiences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Holidays_HolidayTypes_Type",
                        column: x => x.Type,
                        principalSchema: "lookups",
                        principalTable: "HolidayTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Holidays_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                schema: "access",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    SchoolRoleId = table.Column<int>(type: "integer", nullable: false),
                    ModuleId = table.Column<int>(type: "integer", nullable: false),
                    CampusId = table.Column<int>(type: "integer", nullable: false),
                    AllCampuses = table.Column<bool>(type: "boolean", nullable: false),
                    CanView = table.Column<bool>(type: "boolean", nullable: false),
                    CanCreate = table.Column<bool>(type: "boolean", nullable: false),
                    CanEdit = table.Column<bool>(type: "boolean", nullable: false),
                    CanDelete = table.Column<bool>(type: "boolean", nullable: false),
                    CanApprove = table.Column<bool>(type: "boolean", nullable: false),
                    CanExport = table.Column<bool>(type: "boolean", nullable: false),
                    CanViewHistory = table.Column<bool>(type: "boolean", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.UniqueConstraint("AK_RolePermissions_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_RolePermissions_NeedView", "\"CanView\" OR NOT (\"CanCreate\" OR \"CanEdit\" OR \"CanDelete\" OR \"CanApprove\" OR \"CanExport\" OR \"CanViewHistory\")");
                    table.ForeignKey(
                        name: "FK_RolePermissions_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "modules",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RolePermissions_SchoolRoles_SchoolRoleId",
                        column: x => x.SchoolRoleId,
                        principalSchema: "access",
                        principalTable: "SchoolRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchoolPhotos",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    CampusId = table.Column<int>(type: "integer", nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Caption = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Content = table.Column<byte[]>(type: "bytea", nullable: false),
                    Version = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolPhotos", x => x.Id);
                    table.UniqueConstraint("AK_SchoolPhotos_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_SchoolPhotos_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolPhotos_PhotoKinds_Kind",
                        column: x => x.Kind,
                        principalSchema: "lookups",
                        principalTable: "PhotoKinds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolPhotos_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchoolReviews",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    CampusId = table.Column<int>(type: "integer", nullable: true),
                    ReviewerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Relation = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModeratedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ModeratedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Reply = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RepliedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolReviews", x => x.Id);
                    table.UniqueConstraint("AK_SchoolReviews_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_SchoolReviews_Rating", "\"Rating\" BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_SchoolReviews_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolReviews_ReviewStatuses_Status",
                        column: x => x.Status,
                        principalSchema: "lookups",
                        principalTable: "ReviewStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolReviews_ReviewerRelations_Relation",
                        column: x => x.Relation,
                        principalSchema: "lookups",
                        principalTable: "ReviewerRelations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolReviews_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shifts",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    CampusId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.Id);
                    table.UniqueConstraint("AK_Shifts_PublicId", x => x.PublicId);
                    table.CheckConstraint("CK_Shifts_Times", "\"EndTime\" > \"StartTime\"");
                    table.ForeignKey(
                        name: "FK_Shifts_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shifts_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantModules",
                schema: "modules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    ModuleId = table.Column<int>(type: "integer", nullable: false),
                    SubscriptionId = table.Column<int>(type: "integer", nullable: true),
                    CampusId = table.Column<int>(type: "integer", nullable: false),
                    IsSchoolWide = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ValidFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ValidTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantModules", x => x.Id);
                    table.UniqueConstraint("AK_TenantModules_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_TenantModules_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantModules_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "modules",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantModules_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantModules_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalSchema: "billing",
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantModules_TenantModuleStatuses_Status",
                        column: x => x.Status,
                        principalSchema: "lookups",
                        principalTable: "TenantModuleStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserCampuses",
                schema: "access",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    UserDetailsId = table.Column<int>(type: "integer", nullable: false),
                    CampusId = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCampuses", x => x.Id);
                    table.UniqueConstraint("AK_UserCampuses_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_UserCampuses_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCampuses_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCampuses_UserDetails_UserDetailsId",
                        column: x => x.UserDetailsId,
                        principalSchema: "schools",
                        principalTable: "UserDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampusVisits",
                schema: "admissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    EnquiryId = table.Column<int>(type: "integer", nullable: false),
                    CampusId = table.Column<int>(type: "integer", nullable: false),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampusVisits", x => x.Id);
                    table.UniqueConstraint("AK_CampusVisits_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_CampusVisits_AdmissionEnquiries_EnquiryId",
                        column: x => x.EnquiryId,
                        principalSchema: "admissions",
                        principalTable: "AdmissionEnquiries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampusVisits_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampusVisits_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampusVisits_VisitStatuses_Status",
                        column: x => x.Status,
                        principalSchema: "lookups",
                        principalTable: "VisitStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EnquiryFollowUps",
                schema: "admissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    EnquiryId = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryFollowUps", x => x.Id);
                    table.UniqueConstraint("AK_EnquiryFollowUps_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_EnquiryFollowUps_AdmissionEnquiries_EnquiryId",
                        column: x => x.EnquiryId,
                        principalSchema: "admissions",
                        principalTable: "AdmissionEnquiries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnquiryFollowUps_FollowUpKinds_Kind",
                        column: x => x.Kind,
                        principalSchema: "lookups",
                        principalTable: "FollowUpKinds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EnquiryFollowUps_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalSchema: "schools",
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    ClassOfferingId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TeacherUserDetailsId = table.Column<int>(type: "integer", nullable: false),
                    CampusId = table.Column<int>(type: "integer", nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.UniqueConstraint("AK_Courses_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_Courses_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Courses_ClassOfferings_ClassOfferingId",
                        column: x => x.ClassOfferingId,
                        principalSchema: "schools",
                        principalTable: "ClassOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Courses_UserDetails_TeacherUserDetailsId",
                        column: x => x.TeacherUserDetailsId,
                        principalSchema: "schools",
                        principalTable: "UserDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Enrolments",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    StudentUserDetailsId = table.Column<int>(type: "integer", nullable: false),
                    ClassOfferingId = table.Column<int>(type: "integer", nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrolments", x => x.Id);
                    table.UniqueConstraint("AK_Enrolments_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_Enrolments_ClassOfferings_ClassOfferingId",
                        column: x => x.ClassOfferingId,
                        principalSchema: "schools",
                        principalTable: "ClassOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrolments_UserDetails_StudentUserDetailsId",
                        column: x => x.StudentUserDetailsId,
                        principalSchema: "schools",
                        principalTable: "UserDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Assessments",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Weight = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    MaximumScore = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.Id);
                    table.UniqueConstraint("AK_Assessments_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_Assessments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalSchema: "academics",
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimetableSlots",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    CampusId = table.Column<int>(type: "integer", nullable: true),
                    Day = table.Column<int>(type: "integer", nullable: false),
                    Start = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    End = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Room = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimetableSlots", x => x.Id);
                    table.UniqueConstraint("AK_TimetableSlots_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_TimetableSlots_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalSchema: "schools",
                        principalTable: "Campuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimetableSlots_Courses_CourseId",
                        column: x => x.CourseId,
                        principalSchema: "academics",
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimetableSlots_Weekdays_Day",
                        column: x => x.Day,
                        principalSchema: "lookups",
                        principalTable: "Weekdays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Scores",
                schema: "academics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    AssessmentId = table.Column<int>(type: "integer", nullable: false),
                    EnrolmentId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    PublicId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scores", x => x.Id);
                    table.UniqueConstraint("AK_Scores_PublicId", x => x.PublicId);
                    table.ForeignKey(
                        name: "FK_Scores_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "academics",
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Scores_Enrolments_EnrolmentId",
                        column: x => x.EnrolmentId,
                        principalSchema: "academics",
                        principalTable: "Enrolments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "AcademicYearStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Planned" },
                    { 2, "Active" },
                    { 3, "Closed" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "AddOnRequestStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Pending" },
                    { 2, "Approved" },
                    { 3, "Rejected" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "AdmissionStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Open" },
                    { 2, "Closed" },
                    { 3, "WaitingList" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "BillingCycles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Monthly" },
                    { 2, "Yearly" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "BrandingImageKinds",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Logo" },
                    { 2, "Favicon" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "CalendarEventAudiences",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "All" },
                    { 2, "Students" },
                    { 3, "Staff" },
                    { 4, "Parents" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "CalendarEventTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "ParentTeacherMeeting" },
                    { 2, "Sports" },
                    { 3, "Exam" },
                    { 4, "Function" },
                    { 5, "Other" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "CampusKinds",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Main" },
                    { 2, "Other" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "ChangeAuditActions",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Added" },
                    { 2, "Modified" },
                    { 3, "Removed" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "ClassLevels",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Playgroup" },
                    { 2, "Nursery" },
                    { 3, "Prep" },
                    { 4, "Class1" },
                    { 5, "Class2" },
                    { 6, "Class3" },
                    { 7, "Class4" },
                    { 8, "Class5" },
                    { 9, "Class6" },
                    { 10, "Class7" },
                    { 11, "Class8" },
                    { 12, "Class9" },
                    { 13, "Class10" },
                    { 14, "FirstYear" },
                    { 15, "SecondYear" },
                    { 16, "OLevel" },
                    { 17, "ALevel" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "CurriculumKinds",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Matric" },
                    { 2, "Intermediate" },
                    { 3, "FederalBoard" },
                    { 4, "Cambridge" },
                    { 5, "InternationalBaccalaureate" },
                    { 6, "AmericanDiploma" },
                    { 7, "Montessori" },
                    { 8, "Religious" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "DataRegions",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "SouthAsia" },
                    { 2, "MiddleEast" },
                    { 3, "NorthAmerica" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "DaysOfWeek",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "Sunday" },
                    { 1, "Monday" },
                    { 2, "Tuesday" },
                    { 3, "Wednesday" },
                    { 4, "Thursday" },
                    { 5, "Friday" },
                    { 6, "Saturday" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "EnquiryChannels",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Online" },
                    { 2, "Phone" },
                    { 3, "InPerson" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "EnquiryStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "New" },
                    { 2, "Contacted" },
                    { 3, "VisitScheduled" },
                    { 4, "Applied" },
                    { 5, "Enrolled" },
                    { 6, "Closed" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "EventAudiences",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "All" },
                    { 2, "Staff" },
                    { 3, "Parents" },
                    { 4, "Students" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "FacilityKinds",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Transport" },
                    { 2, "Hostel" },
                    { 3, "Library" },
                    { 4, "ScienceLab" },
                    { 5, "ComputerLab" },
                    { 6, "SportsGround" },
                    { 7, "Cafeteria" },
                    { 8, "PrayerArea" },
                    { 9, "MedicalRoom" },
                    { 10, "Cctv" },
                    { 11, "AirConditioning" },
                    { 12, "Playground" },
                    { 13, "Auditorium" },
                    { 14, "SwimmingPool" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "FollowUpKinds",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Call" },
                    { 2, "Email" },
                    { 3, "Visit" },
                    { 4, "Note" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "Genders",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Male" },
                    { 2, "Female" },
                    { 3, "Other" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "HolidayAudiences",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Students" },
                    { 2, "Staff" },
                    { 3, "Both" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "HolidayTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Public" },
                    { 2, "Religious" },
                    { 3, "Vacation" },
                    { 4, "School" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "InvoiceStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Open" },
                    { 2, "Paid" },
                    { 3, "Void" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "ModuleCodes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "None" },
                    { 1, "School" },
                    { 2, "Campus" },
                    { 3, "AcademicSetup" },
                    { 4, "UsersRoles" },
                    { 5, "Students" },
                    { 6, "Staff" },
                    { 7, "Admissions" },
                    { 8, "Attendance" },
                    { 9, "Fees" },
                    { 10, "ParentCommunication" },
                    { 11, "Exams" },
                    { 12, "Timetable" },
                    { 13, "HrPayroll" },
                    { 14, "Transport" },
                    { 15, "Library" },
                    { 16, "Lms" },
                    { 17, "Hostel" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "PaymentMethods",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "BankTransfer" },
                    { 2, "Cash" },
                    { 3, "Cheque" },
                    { 4, "MobileWallet" },
                    { 5, "Other" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "PhotoKinds",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Campus" },
                    { 2, "Classroom" },
                    { 3, "Facility" },
                    { 4, "Event" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "ReviewStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Pending" },
                    { 2, "Approved" },
                    { 3, "Rejected" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "ReviewerRelations",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Parent" },
                    { 2, "Student" },
                    { 3, "Alumni" },
                    { 4, "Other" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "SchoolGenderPolicies",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "BoysOnly" },
                    { 2, "GirlsOnly" },
                    { 3, "CoEducation" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "SchoolRequestStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Pending" },
                    { 2, "Approved" },
                    { 3, "Rejected" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "SignInSucceeded" },
                    { 2, "SignInFailed" },
                    { 3, "SignInLockedOut" },
                    { 4, "SignedOut" },
                    { 5, "AccessDenied" },
                    { 6, "TenantMismatch" },
                    { 7, "BrandingChanged" },
                    { 8, "CountryChanged" },
                    { 9, "SchoolCountryChanged" },
                    { 10, "ModuleDefaultChanged" },
                    { 11, "SchoolModuleChanged" },
                    { 12, "ModuleChanged" },
                    { 13, "ModuleCreated" },
                    { 14, "ModuleDeleted" },
                    { 15, "ModuleDependencyAdded" },
                    { 16, "ModuleDependencyRemoved" },
                    { 17, "PlanCreated" },
                    { 18, "PlanChanged" },
                    { 19, "PlanDeleted" },
                    { 20, "PriceBookCreated" },
                    { 21, "PriceBookChanged" },
                    { 22, "PriceBookDeleted" },
                    { 23, "PriceChanged" },
                    { 24, "SubscriptionCreated" },
                    { 25, "DataRegionChanged" },
                    { 26, "RoleCreated" },
                    { 27, "RoleChanged" },
                    { 28, "RoleDeleted" },
                    { 29, "InvoiceCreated" },
                    { 30, "InvoiceVoided" },
                    { 31, "PaymentRecorded" },
                    { 32, "SchoolTaxRateChanged" },
                    { 33, "SchoolCreated" },
                    { 34, "UserCreated" },
                    { 35, "PasswordChanged" },
                    { 36, "SubscriptionChanged" },
                    { 37, "AddOnRequested" },
                    { 38, "AddOnRequestDecided" },
                    { 39, "InvoiceMarkedOverdue" },
                    { 40, "SchoolProfileChanged" },
                    { 41, "CampusCreated" },
                    { 42, "CampusUpdated" },
                    { 43, "CampusDeleted" },
                    { 44, "SchoolDeleted" },
                    { 45, "CampusSettingsChanged" },
                    { 46, "UserDetailsCreated" },
                    { 47, "SchoolRequestApproved" },
                    { 48, "SchoolRequestRejected" },
                    { 49, "UserDetailsUpdated" },
                    { 50, "UserDetailsDeleted" },
                    { 51, "PublicPageProfileChanged" },
                    { 52, "PublicPagePublished" },
                    { 53, "PublicPageUnpublished" },
                    { 54, "SchoolCurriculaChanged" },
                    { 55, "SchoolFacilityChanged" },
                    { 56, "SchoolPhotoUploaded" },
                    { 57, "SchoolPhotoChanged" },
                    { 58, "SchoolPhotoDeleted" },
                    { 59, "ClassOfferingChanged" },
                    { 60, "AcademicYearChanged" },
                    { 61, "ShiftChanged" },
                    { 62, "SchoolCalendarChanged" },
                    { 63, "SchoolDetailsChanged" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "SecurityAuditOutcomes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Success" },
                    { 2, "Failure" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "StateTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "State" },
                    { 2, "Province" },
                    { 3, "Prefecture" },
                    { 4, "Emirate" },
                    { 5, "Governorate" },
                    { 6, "Territory" },
                    { 7, "Region" },
                    { 8, "Municipality" },
                    { 9, "District" },
                    { 10, "AdministeredArea" },
                    { 11, "FederalCapitalTerritory" },
                    { 12, "OutlyingArea" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "SubscriptionStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Trial" },
                    { 2, "Active" },
                    { 3, "PastDue" },
                    { 4, "Suspended" },
                    { 5, "Cancelled" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "TenantModuleStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Active" },
                    { 2, "Suspended" },
                    { 3, "Expired" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "UserTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Principal" },
                    { 2, "Teacher" },
                    { 3, "Staff" },
                    { 4, "Student" },
                    { 5, "Parent" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "VisitStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Requested" },
                    { 2, "Confirmed" },
                    { 3, "Completed" },
                    { 4, "Cancelled" },
                    { 5, "NoShow" }
                });

            migrationBuilder.InsertData(
                schema: "lookups",
                table: "Weekdays",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Monday" },
                    { 2, "Tuesday" },
                    { 3, "Wednesday" },
                    { 4, "Thursday" },
                    { 5, "Friday" },
                    { 6, "Saturday" },
                    { 7, "Sunday" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_SchoolId_Name",
                schema: "academics",
                table: "AcademicYears",
                columns: new[] { "SchoolId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_SingleCurrentPerSchool",
                schema: "academics",
                table: "AcademicYears",
                column: "SchoolId",
                unique: true,
                filter: "\"IsCurrent\" AND \"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_Status",
                schema: "academics",
                table: "AcademicYears",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRoles_SchoolId_UserId_SchoolRoleId",
                schema: "access",
                table: "AccountRoles",
                columns: new[] { "SchoolId", "UserId", "SchoolRoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountRoles_SchoolRoleId",
                schema: "access",
                table: "AccountRoles",
                column: "SchoolRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRoles_UserId",
                schema: "access",
                table: "AccountRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AddOnRequests_ModuleId",
                schema: "billing",
                table: "AddOnRequests",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_AddOnRequests_SchoolId_ModuleId",
                schema: "billing",
                table: "AddOnRequests",
                columns: new[] { "SchoolId", "ModuleId" },
                unique: true,
                filter: "\"Status\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_AddOnRequests_Status",
                schema: "billing",
                table: "AddOnRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AdmissionEnquiries_AssignedToUserDetailsId",
                schema: "admissions",
                table: "AdmissionEnquiries",
                column: "AssignedToUserDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_AdmissionEnquiries_CampusId",
                schema: "admissions",
                table: "AdmissionEnquiries",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_AdmissionEnquiries_Channel",
                schema: "admissions",
                table: "AdmissionEnquiries",
                column: "Channel");

            migrationBuilder.CreateIndex(
                name: "IX_AdmissionEnquiries_Level",
                schema: "admissions",
                table: "AdmissionEnquiries",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_AdmissionEnquiries_SchoolId_Status_SubmittedAt",
                schema: "admissions",
                table: "AdmissionEnquiries",
                columns: new[] { "SchoolId", "Status", "SubmittedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AdmissionEnquiries_Status",
                schema: "admissions",
                table: "AdmissionEnquiries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "identity",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "identity",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "identity",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "identity",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: "identity",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "identity",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SchoolId",
                schema: "identity",
                table: "AspNetUsers",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SchoolId_UserDetailsId",
                schema: "identity",
                table: "AspNetUsers",
                columns: new[] { "SchoolId", "UserDetailsId" },
                unique: true,
                filter: "\"UserDetailsId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "identity",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_CourseId",
                schema: "academics",
                table: "Assessments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_SchoolId_CourseId",
                schema: "academics",
                table: "Assessments",
                columns: new[] { "SchoolId", "CourseId" });

            migrationBuilder.CreateIndex(
                name: "IX_BrandingImages_Kind",
                schema: "schools",
                table: "BrandingImages",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_BrandingImages_SchoolId_Kind",
                schema: "schools",
                table: "BrandingImages",
                columns: new[] { "SchoolId", "Kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_Audience",
                schema: "academics",
                table: "CalendarEvents",
                column: "Audience");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_CampusId",
                schema: "academics",
                table: "CalendarEvents",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_SchoolId_EventDate",
                schema: "academics",
                table: "CalendarEvents",
                columns: new[] { "SchoolId", "EventDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_Type",
                schema: "academics",
                table: "CalendarEvents",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Campuses_CityId",
                schema: "schools",
                table: "Campuses",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Campuses_GenderPolicy",
                schema: "schools",
                table: "Campuses",
                column: "GenderPolicy");

            migrationBuilder.CreateIndex(
                name: "IX_Campuses_Kind",
                schema: "schools",
                table: "Campuses",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_Campuses_PrincipalUserDetailsId",
                schema: "schools",
                table: "Campuses",
                column: "PrincipalUserDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Campuses_SchoolId_Code",
                schema: "schools",
                table: "Campuses",
                columns: new[] { "SchoolId", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Campuses_SingleMainPerSchool",
                schema: "schools",
                table: "Campuses",
                column: "SchoolId",
                unique: true,
                filter: "\"Kind\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Campuses_StateId",
                schema: "schools",
                table: "Campuses",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_CampusPhones_CampusId_Position",
                schema: "schools",
                table: "CampusPhones",
                columns: new[] { "CampusId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampusPhones_SchoolId",
                schema: "schools",
                table: "CampusPhones",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_CampusVisits_CampusId",
                schema: "admissions",
                table: "CampusVisits",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_CampusVisits_EnquiryId",
                schema: "admissions",
                table: "CampusVisits",
                column: "EnquiryId");

            migrationBuilder.CreateIndex(
                name: "IX_CampusVisits_SchoolId_ScheduledAt",
                schema: "admissions",
                table: "CampusVisits",
                columns: new[] { "SchoolId", "ScheduledAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CampusVisits_Status",
                schema: "admissions",
                table: "CampusVisits",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeAuditEntries_Action",
                schema: "audit",
                table: "ChangeAuditEntries",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeAuditEntries_EntityType_EntityId",
                schema: "audit",
                table: "ChangeAuditEntries",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChangeAuditEntries_OccurredAt",
                schema: "audit",
                table: "ChangeAuditEntries",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeAuditEntries_SchoolId",
                schema: "audit",
                table: "ChangeAuditEntries",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_StateId_Name",
                schema: "schools",
                table: "Cities",
                columns: new[] { "StateId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassOfferings_CampusId",
                schema: "schools",
                table: "ClassOfferings",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassOfferings_Level",
                schema: "schools",
                table: "ClassOfferings",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_ClassOfferings_SchoolId_CampusId_Level_AcademicYear",
                schema: "schools",
                table: "ClassOfferings",
                columns: new[] { "SchoolId", "CampusId", "Level", "AcademicYear" },
                unique: true,
                filter: "\"IsDeleted\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ClassOfferings_Status",
                schema: "schools",
                table: "ClassOfferings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Code",
                schema: "schools",
                table: "Countries",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Iso3",
                schema: "schools",
                table: "Countries",
                column: "Iso3",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_WeekStartDay",
                schema: "schools",
                table: "Countries",
                column: "WeekStartDay");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CampusId",
                schema: "academics",
                table: "Courses",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_ClassOfferingId",
                schema: "academics",
                table: "Courses",
                column: "ClassOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_SchoolId_ClassOfferingId",
                schema: "academics",
                table: "Courses",
                columns: new[] { "SchoolId", "ClassOfferingId" });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_SchoolId_TeacherUserDetailsId",
                schema: "academics",
                table: "Courses",
                columns: new[] { "SchoolId", "TeacherUserDetailsId" });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_TeacherUserDetailsId",
                schema: "academics",
                table: "Courses",
                column: "TeacherUserDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryFollowUps_EnquiryId_OccurredAt",
                schema: "admissions",
                table: "EnquiryFollowUps",
                columns: new[] { "EnquiryId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryFollowUps_Kind",
                schema: "admissions",
                table: "EnquiryFollowUps",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryFollowUps_SchoolId",
                schema: "admissions",
                table: "EnquiryFollowUps",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_ClassOfferingId",
                schema: "academics",
                table: "Enrolments",
                column: "ClassOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_SchoolId_StudentUserDetailsId_ClassOfferingId",
                schema: "academics",
                table: "Enrolments",
                columns: new[] { "SchoolId", "StudentUserDetailsId", "ClassOfferingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_StudentUserDetailsId",
                schema: "academics",
                table: "Enrolments",
                column: "StudentUserDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_GradingBands_GradingScaleId",
                schema: "academics",
                table: "GradingBands",
                column: "GradingScaleId");

            migrationBuilder.CreateIndex(
                name: "IX_GradingBands_SchoolId_GradingScaleId",
                schema: "academics",
                table: "GradingBands",
                columns: new[] { "SchoolId", "GradingScaleId" });

            migrationBuilder.CreateIndex(
                name: "IX_GradingScales_SchoolId_Name",
                schema: "academics",
                table: "GradingScales",
                columns: new[] { "SchoolId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_AppliesTo",
                schema: "academics",
                table: "Holidays",
                column: "AppliesTo");

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_CampusId",
                schema: "academics",
                table: "Holidays",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_SchoolId_StartDate",
                schema: "academics",
                table: "Holidays",
                columns: new[] { "SchoolId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_Type",
                schema: "academics",
                table: "Holidays",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_InvoiceId",
                schema: "billing",
                table: "InvoiceLines",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_SchoolId",
                schema: "billing",
                table: "InvoiceLines",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Number",
                schema: "billing",
                table: "Invoices",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_OneOpenPerSubscription",
                schema: "billing",
                table: "Invoices",
                column: "SubscriptionId",
                unique: true,
                filter: "\"Status\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PeriodInvoicedOnce",
                schema: "billing",
                table: "Invoices",
                columns: new[] { "SubscriptionId", "PeriodStart" },
                unique: true,
                filter: "\"Status\" <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SchoolId",
                schema: "billing",
                table: "Invoices",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Status",
                schema: "billing",
                table: "Invoices",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleDependencies_ModuleId_RequiresModuleId",
                schema: "modules",
                table: "ModuleDependencies",
                columns: new[] { "ModuleId", "RequiresModuleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleDependencies_RequiresModuleId",
                schema: "modules",
                table: "ModuleDependencies",
                column: "RequiresModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Code",
                schema: "modules",
                table: "Modules",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SchoolEventId",
                schema: "communication",
                table: "Notifications",
                column: "SchoolEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SchoolId_RecipientAccountId_Created",
                schema: "communication",
                table: "Notifications",
                columns: new[] { "SchoolId", "RecipientAccountId", "Created" });

            migrationBuilder.CreateIndex(
                name: "IX_ParentStudents_ParentUserDetailsId",
                schema: "schools",
                table: "ParentStudents",
                column: "ParentUserDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentStudents_SchoolId_ParentUserDetailsId_StudentUserDeta~",
                schema: "schools",
                table: "ParentStudents",
                columns: new[] { "SchoolId", "ParentUserDetailsId", "StudentUserDetailsId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParentStudents_SchoolId_StudentUserDetailsId",
                schema: "schools",
                table: "ParentStudents",
                columns: new[] { "SchoolId", "StudentUserDetailsId" });

            migrationBuilder.CreateIndex(
                name: "IX_ParentStudents_StudentUserDetailsId",
                schema: "schools",
                table: "ParentStudents",
                column: "StudentUserDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceId",
                schema: "billing",
                table: "Payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Method",
                schema: "billing",
                table: "Payments",
                column: "Method");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SchoolId",
                schema: "billing",
                table: "Payments",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanModules_ModuleId",
                schema: "billing",
                table: "PlanModules",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanModules_PlanId_ModuleId",
                schema: "billing",
                table: "PlanModules",
                columns: new[] { "PlanId", "ModuleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plans_Code",
                schema: "billing",
                table: "Plans",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PriceBookItems_BillingCycle",
                schema: "billing",
                table: "PriceBookItems",
                column: "BillingCycle");

            migrationBuilder.CreateIndex(
                name: "IX_PriceBookItems_ModuleId",
                schema: "billing",
                table: "PriceBookItems",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceBookItems_PlanId",
                schema: "billing",
                table: "PriceBookItems",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceBookItems_PriceBookId_ModuleId_BillingCycle",
                schema: "billing",
                table: "PriceBookItems",
                columns: new[] { "PriceBookId", "ModuleId", "BillingCycle" },
                unique: true,
                filter: "\"ModuleId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PriceBookItems_PriceBookId_PlanId_BillingCycle",
                schema: "billing",
                table: "PriceBookItems",
                columns: new[] { "PriceBookId", "PlanId", "BillingCycle" },
                unique: true,
                filter: "\"PlanId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PriceBooks_CountryCode",
                schema: "billing",
                table: "PriceBooks",
                column: "CountryCode",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_RenewalReminders_SchoolId",
                schema: "billing",
                table: "RenewalReminders",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_RenewalReminders_SubscriptionId_PeriodEnd_DaysBefore",
                schema: "billing",
                table: "RenewalReminders",
                columns: new[] { "SubscriptionId", "PeriodEnd", "DaysBefore" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RenewalReminders_Unsent",
                schema: "billing",
                table: "RenewalReminders",
                column: "QueuedAt",
                filter: "\"SentAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_CampusId",
                schema: "access",
                table: "RolePermissions",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_ModuleId",
                schema: "access",
                table: "RolePermissions",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_SchoolId",
                schema: "access",
                table: "RolePermissions",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_SchoolRoleId_ModuleId_CampusId",
                schema: "access",
                table: "RolePermissions",
                columns: new[] { "SchoolRoleId", "ModuleId", "CampusId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCurricula_Kind",
                schema: "schools",
                table: "SchoolCurricula",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCurricula_SchoolId_Kind",
                schema: "schools",
                table: "SchoolCurricula",
                columns: new[] { "SchoolId", "Kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolEvents_Audience",
                schema: "communication",
                table: "SchoolEvents",
                column: "Audience");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolEvents_SchoolId_StartsAt",
                schema: "communication",
                table: "SchoolEvents",
                columns: new[] { "SchoolId", "StartsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolFacilities_Kind",
                schema: "schools",
                table: "SchoolFacilities",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolFacilities_SchoolId_Kind",
                schema: "schools",
                table: "SchoolFacilities",
                columns: new[] { "SchoolId", "Kind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolPhotos_CampusId",
                schema: "schools",
                table: "SchoolPhotos",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolPhotos_Kind",
                schema: "schools",
                table: "SchoolPhotos",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolPhotos_SchoolId_SortOrder",
                schema: "schools",
                table: "SchoolPhotos",
                columns: new[] { "SchoolId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolProfiles_GenderPolicy",
                schema: "schools",
                table: "SchoolProfiles",
                column: "GenderPolicy");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolProfiles_SchoolId",
                schema: "schools",
                table: "SchoolProfiles",
                column: "SchoolId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolRequests_Status",
                schema: "schools",
                table: "SchoolRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolReviews_CampusId",
                schema: "schools",
                table: "SchoolReviews",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolReviews_Relation",
                schema: "schools",
                table: "SchoolReviews",
                column: "Relation");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolReviews_SchoolId_Status_SubmittedAt",
                schema: "schools",
                table: "SchoolReviews",
                columns: new[] { "SchoolId", "Status", "SubmittedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolReviews_Status",
                schema: "schools",
                table: "SchoolReviews",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolRoles_SchoolId_Name",
                schema: "access",
                table: "SchoolRoles",
                columns: new[] { "SchoolId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schools_CityId",
                schema: "schools",
                table: "Schools",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_CountryId",
                schema: "schools",
                table: "Schools",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_DataRegion",
                schema: "schools",
                table: "Schools",
                column: "DataRegion");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_Domain",
                schema: "schools",
                table: "Schools",
                column: "Domain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schools_SingleMasterTenant",
                schema: "schools",
                table: "Schools",
                column: "IsMaster",
                unique: true,
                filter: "\"IsMaster\"");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_TenantId",
                schema: "schools",
                table: "Schools",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_AssessmentId_EnrolmentId",
                schema: "academics",
                table: "Scores",
                columns: new[] { "AssessmentId", "EnrolmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Scores_EnrolmentId",
                schema: "academics",
                table: "Scores",
                column: "EnrolmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditEvents_ActorUserId",
                schema: "audit",
                table: "SecurityAuditEvents",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditEvents_EventType",
                schema: "audit",
                table: "SecurityAuditEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditEvents_OccurredAt",
                schema: "audit",
                table: "SecurityAuditEvents",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditEvents_Outcome",
                schema: "audit",
                table: "SecurityAuditEvents",
                column: "Outcome");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_CampusId_Name",
                schema: "academics",
                table: "Shifts",
                columns: new[] { "CampusId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_SchoolId",
                schema: "academics",
                table: "Shifts",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_States_CountryId_Name",
                schema: "schools",
                table: "States",
                columns: new[] { "CountryId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_States_IsoCode",
                schema: "schools",
                table: "States",
                column: "IsoCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_States_Type",
                schema: "schools",
                table: "States",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_Gender",
                schema: "schools",
                table: "StudentProfiles",
                column: "Gender");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_SchoolId",
                schema: "schools",
                table: "StudentProfiles",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_UserDetailsId",
                schema: "schools",
                table: "StudentProfiles",
                column: "UserDetailsId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_BillingCycle",
                schema: "billing",
                table: "Subscriptions",
                column: "BillingCycle");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PlanId",
                schema: "billing",
                table: "Subscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PriceBookId",
                schema: "billing",
                table: "Subscriptions",
                column: "PriceBookId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_SchoolId",
                schema: "billing",
                table: "Subscriptions",
                column: "SchoolId",
                unique: true,
                filter: "\"Status\" <> 5");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_Status",
                schema: "billing",
                table: "Subscriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TenantModules_CampusId",
                schema: "modules",
                table: "TenantModules",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantModules_DefaultGrantOnce",
                schema: "modules",
                table: "TenantModules",
                columns: new[] { "SchoolId", "ModuleId", "CampusId" },
                unique: true,
                filter: "\"SubscriptionId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TenantModules_ModuleId",
                schema: "modules",
                table: "TenantModules",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantModules_SchoolId_ModuleId",
                schema: "modules",
                table: "TenantModules",
                columns: new[] { "SchoolId", "ModuleId" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantModules_Status",
                schema: "modules",
                table: "TenantModules",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TenantModules_SubscriptionId_ModuleId_CampusId",
                schema: "modules",
                table: "TenantModules",
                columns: new[] { "SubscriptionId", "ModuleId", "CampusId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_SingleMasterTenant",
                schema: "schools",
                table: "Tenants",
                column: "IsMaster",
                unique: true,
                filter: "\"IsMaster\"");

            migrationBuilder.CreateIndex(
                name: "IX_Terms_AcademicYearId_Sequence",
                schema: "academics",
                table: "Terms",
                columns: new[] { "AcademicYearId", "Sequence" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Terms_SchoolId",
                schema: "academics",
                table: "Terms",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableSlots_CampusId",
                schema: "academics",
                table: "TimetableSlots",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableSlots_CourseId",
                schema: "academics",
                table: "TimetableSlots",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableSlots_Day",
                schema: "academics",
                table: "TimetableSlots",
                column: "Day");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableSlots_SchoolId_CourseId",
                schema: "academics",
                table: "TimetableSlots",
                columns: new[] { "SchoolId", "CourseId" });

            migrationBuilder.CreateIndex(
                name: "IX_TimetableSlots_SchoolId_Day",
                schema: "academics",
                table: "TimetableSlots",
                columns: new[] { "SchoolId", "Day" });

            migrationBuilder.CreateIndex(
                name: "IX_UserCampuses_CampusId",
                schema: "access",
                table: "UserCampuses",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCampuses_SchoolId",
                schema: "access",
                table: "UserCampuses",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCampuses_UserDetailsId_CampusId",
                schema: "access",
                table: "UserCampuses",
                columns: new[] { "UserDetailsId", "CampusId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDetails_SchoolId_UserType",
                schema: "schools",
                table: "UserDetails",
                columns: new[] { "SchoolId", "UserType" });

            migrationBuilder.CreateIndex(
                name: "IX_UserDetails_UserType",
                schema: "schools",
                table: "UserDetails",
                column: "UserType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountRoles",
                schema: "access");

            migrationBuilder.DropTable(
                name: "AddOnRequests",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "BrandingImages",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "CalendarEvents",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "CampusPhones",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "CampusVisits",
                schema: "admissions");

            migrationBuilder.DropTable(
                name: "ChangeAuditEntries",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "EnquiryFollowUps",
                schema: "admissions");

            migrationBuilder.DropTable(
                name: "GradingBands",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "Holidays",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "InvoiceLines",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "ModuleDependencies",
                schema: "modules");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "communication");

            migrationBuilder.DropTable(
                name: "ParentStudents",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "Payments",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "PlanModules",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "PriceBookItems",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "RenewalReminders",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "RolePermissions",
                schema: "access");

            migrationBuilder.DropTable(
                name: "SchoolCurricula",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "SchoolFacilities",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "SchoolPhotos",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "SchoolProfiles",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "SchoolRequests",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "SchoolReviews",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "Scores",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "SecurityAuditEvents",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "Shifts",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "StudentProfiles",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "TenantModules",
                schema: "modules");

            migrationBuilder.DropTable(
                name: "Terms",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "TimetableSlots",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "UserCampuses",
                schema: "access");

            migrationBuilder.DropTable(
                name: "AddOnRequestStatuses",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "AspNetRoles",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "AspNetUsers",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "BrandingImageKinds",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "CalendarEventAudiences",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "CalendarEventTypes",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "VisitStatuses",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "ChangeAuditActions",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "AdmissionEnquiries",
                schema: "admissions");

            migrationBuilder.DropTable(
                name: "FollowUpKinds",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "GradingScales",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "HolidayAudiences",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "HolidayTypes",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "SchoolEvents",
                schema: "communication");

            migrationBuilder.DropTable(
                name: "Invoices",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "PaymentMethods",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "SchoolRoles",
                schema: "access");

            migrationBuilder.DropTable(
                name: "CurriculumKinds",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "FacilityKinds",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "PhotoKinds",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "SchoolRequestStatuses",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "ReviewStatuses",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "ReviewerRelations",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "Assessments",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "Enrolments",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "SecurityAuditEventTypes",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "SecurityAuditOutcomes",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "Genders",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "Modules",
                schema: "modules");

            migrationBuilder.DropTable(
                name: "TenantModuleStatuses",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "AcademicYears",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "Weekdays",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "EnquiryChannels",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "EnquiryStatuses",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "EventAudiences",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "InvoiceStatuses",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "Subscriptions",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "Courses",
                schema: "academics");

            migrationBuilder.DropTable(
                name: "ModuleCodes",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "AcademicYearStatuses",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "BillingCycles",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "Plans",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "PriceBooks",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "SubscriptionStatuses",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "ClassOfferings",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "AdmissionStatuses",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "Campuses",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "ClassLevels",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "CampusKinds",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "SchoolGenderPolicies",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "UserDetails",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "Schools",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "UserTypes",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "Cities",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "DataRegions",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "Tenants",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "States",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "Countries",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "StateTypes",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "DaysOfWeek",
                schema: "lookups");

            migrationBuilder.DropSequence(
                name: "InvoiceNumbers",
                schema: "billing");
        }
    }
}

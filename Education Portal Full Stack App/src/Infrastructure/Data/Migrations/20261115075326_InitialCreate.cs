using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EducationPortal.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "billing");

            migrationBuilder.EnsureSchema(
                name: "lookups");

            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.EnsureSchema(
                name: "schools");

            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.EnsureSchema(
                name: "modules");

            migrationBuilder.EnsureSchema(
                name: "access");

            migrationBuilder.CreateSequence<int>(
                name: "InvoiceNumbers",
                schema: "billing");

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
                name: "Modules",
                schema: "modules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                name: "Schools",
                schema: "schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Domain = table.Column<string>(type: "character varying(253)", maxLength: 253, nullable: false),
                    IsMaster = table.Column<bool>(type: "boolean", nullable: false),
                    AccentColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    SidebarColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    CountryId = table.Column<int>(type: "integer", nullable: true),
                    DataRegion = table.Column<int>(type: "integer", nullable: true),
                    TaxRateOverride = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    CampusCodePrefixEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CampusCodePrefix = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
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
                    table.CheckConstraint("CK_Schools_TaxRateOverrideRange", "\"TaxRateOverride\" IS NULL OR (\"TaxRateOverride\" >= 0 AND \"TaxRateOverride\" <= 100)");
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
                name: "SchoolRoles",
                schema: "access",
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
                name: "RolePermissions",
                schema: "access",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SchoolId = table.Column<int>(type: "integer", nullable: false),
                    SchoolRoleId = table.Column<int>(type: "integer", nullable: false),
                    ModuleId = table.Column<int>(type: "integer", nullable: false),
                    CanView = table.Column<bool>(type: "boolean", nullable: false),
                    CanCreate = table.Column<bool>(type: "boolean", nullable: false),
                    CanEdit = table.Column<bool>(type: "boolean", nullable: false),
                    CanDelete = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.CheckConstraint("CK_RolePermissions_NeedView", "\"CanView\" OR NOT (\"CanCreate\" OR \"CanEdit\" OR \"CanDelete\")");
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
                    PrincipalUserDetailsId = table.Column<int>(type: "integer", nullable: true),
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
                    { 48, "SchoolRequestRejected" }
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
                    { 3, "Staff" }
                });

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
                name: "UserNameIndex",
                schema: "identity",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

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
                name: "IX_Campuses_CityId",
                schema: "schools",
                table: "Campuses",
                column: "CityId");

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
                name: "IX_RolePermissions_SchoolRoleId_ModuleId",
                schema: "access",
                table: "RolePermissions",
                columns: new[] { "SchoolRoleId", "ModuleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolRequests_Status",
                schema: "schools",
                table: "SchoolRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolRoles_SchoolId_Name",
                schema: "access",
                table: "SchoolRoles",
                columns: new[] { "SchoolId", "Name" },
                unique: true);

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
                name: "CampusPhones",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "ChangeAuditEntries",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "InvoiceLines",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "ModuleDependencies",
                schema: "modules");

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
                name: "SchoolRequests",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "SecurityAuditEvents",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "TenantModules",
                schema: "modules");

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
                name: "ChangeAuditActions",
                schema: "lookups");

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
                name: "SchoolRequestStatuses",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "SecurityAuditEventTypes",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "SecurityAuditOutcomes",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "Campuses",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "Modules",
                schema: "modules");

            migrationBuilder.DropTable(
                name: "TenantModuleStatuses",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "InvoiceStatuses",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "Subscriptions",
                schema: "billing");

            migrationBuilder.DropTable(
                name: "CampusKinds",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "Cities",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "UserDetails",
                schema: "schools");

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
                name: "States",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "Schools",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "UserTypes",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "StateTypes",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "Countries",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "DataRegions",
                schema: "lookups");

            migrationBuilder.DropSequence(
                name: "InvoiceNumbers",
                schema: "billing");
        }
    }
}

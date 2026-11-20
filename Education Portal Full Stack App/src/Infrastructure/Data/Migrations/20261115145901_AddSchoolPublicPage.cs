using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EducationPortal.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolPublicPage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "admissions");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampusVisits",
                schema: "admissions");

            migrationBuilder.DropTable(
                name: "ClassOfferings",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "EnquiryFollowUps",
                schema: "admissions");

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
                name: "SchoolReviews",
                schema: "schools");

            migrationBuilder.DropTable(
                name: "VisitStatuses",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "AdmissionStatuses",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "AdmissionEnquiries",
                schema: "admissions");

            migrationBuilder.DropTable(
                name: "FollowUpKinds",
                schema: "lookups");

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
                name: "SchoolGenderPolicies",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "ReviewStatuses",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "ReviewerRelations",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "ClassLevels",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "EnquiryChannels",
                schema: "lookups");

            migrationBuilder.DropTable(
                name: "EnquiryStatuses",
                schema: "lookups");
        }
    }
}

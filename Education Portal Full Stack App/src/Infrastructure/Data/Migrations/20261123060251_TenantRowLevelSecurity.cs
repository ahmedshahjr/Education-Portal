using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationPortal.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class TenantRowLevelSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "schools",
                table: "UserDetails",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "access",
                table: "UserCampuses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "academics",
                table: "TimetableSlots",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "academics",
                table: "Terms",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "modules",
                table: "TenantModules",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "billing",
                table: "Subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "schools",
                table: "StudentProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "academics",
                table: "Shifts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "academics",
                table: "Scores",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "access",
                table: "SchoolRoles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "schools",
                table: "SchoolReviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "schools",
                table: "SchoolProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "schools",
                table: "SchoolPhotos",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "schools",
                table: "SchoolFacilities",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "communication",
                table: "SchoolEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "schools",
                table: "SchoolCurricula",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "access",
                table: "RolePermissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "billing",
                table: "RenewalReminders",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "billing",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "schools",
                table: "ParentStudents",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "communication",
                table: "Notifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "billing",
                table: "Invoices",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "billing",
                table: "InvoiceLines",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "academics",
                table: "Holidays",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "academics",
                table: "GradingScales",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "academics",
                table: "GradingBands",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "academics",
                table: "Enrolments",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "admissions",
                table: "EnquiryFollowUps",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "academics",
                table: "Courses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "schools",
                table: "ClassOfferings",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "admissions",
                table: "CampusVisits",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "schools",
                table: "CampusPhones",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "schools",
                table: "Campuses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "academics",
                table: "CalendarEvents",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "schools",
                table: "BrandingImages",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "academics",
                table: "Assessments",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "admissions",
                table: "AdmissionEnquiries",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "billing",
                table: "AddOnRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "access",
                table: "AccountRoles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                schema: "academics",
                table: "AcademicYears",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION public.education_portal_set_tenant() RETURNS trigger
                LANGUAGE plpgsql AS $$
                DECLARE
                    school_tenant integer;
                BEGIN
                    -- A row can never move to another school; its tenant always follows its school (ADR-026).
                    IF TG_OP = 'UPDATE' AND NEW."SchoolId" IS DISTINCT FROM OLD."SchoolId" THEN
                        RAISE EXCEPTION 'A row cannot be moved to another school' USING ERRCODE = 'check_violation';
                    END IF;

                    SELECT s."TenantId" INTO school_tenant FROM schools."Schools" AS s WHERE s."Id" = NEW."SchoolId";
                    IF school_tenant IS NULL THEN
                        RAISE EXCEPTION 'The school of the row does not exist' USING ERRCODE = 'foreign_key_violation';
                    END IF;

                    NEW."TenantId" := school_tenant;
                    RETURN NEW;
                END;
                $$;
                """);
            migrationBuilder.Sql("""
                UPDATE "schools"."UserDetails" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "access"."UserCampuses" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "academics"."TimetableSlots" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "academics"."Terms" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "modules"."TenantModules" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "billing"."Subscriptions" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "schools"."StudentProfiles" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "academics"."Shifts" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "academics"."Scores" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "access"."SchoolRoles" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "schools"."SchoolReviews" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "schools"."SchoolProfiles" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "schools"."SchoolPhotos" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "schools"."SchoolFacilities" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "communication"."SchoolEvents" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "schools"."SchoolCurricula" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "access"."RolePermissions" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "billing"."RenewalReminders" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "billing"."Payments" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "schools"."ParentStudents" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "communication"."Notifications" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "billing"."Invoices" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "billing"."InvoiceLines" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "academics"."Holidays" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "academics"."GradingScales" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "academics"."GradingBands" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "academics"."Enrolments" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "admissions"."EnquiryFollowUps" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "academics"."Courses" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "schools"."ClassOfferings" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "admissions"."CampusVisits" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "schools"."CampusPhones" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "schools"."Campuses" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "academics"."CalendarEvents" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "schools"."BrandingImages" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "academics"."Assessments" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "admissions"."AdmissionEnquiries" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "billing"."AddOnRequests" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "access"."AccountRoles" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.Sql("""
                UPDATE "academics"."AcademicYears" AS t SET "TenantId" = s."TenantId" FROM schools."Schools" AS s WHERE s."Id" = t."SchoolId";
                """);
            migrationBuilder.InsertData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                columns: new[] { "Id", "Name" },
                values: new object[] { 64, "MasterTenantAccess" });
            migrationBuilder.CreateIndex(
                name: "IX_UserDetails_TenantId_SchoolId",
                schema: "schools",
                table: "UserDetails",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_UserCampuses_TenantId_SchoolId",
                schema: "access",
                table: "UserCampuses",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_TimetableSlots_TenantId_SchoolId",
                schema: "academics",
                table: "TimetableSlots",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_Terms_TenantId_SchoolId",
                schema: "academics",
                table: "Terms",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_TenantModules_TenantId_SchoolId",
                schema: "modules",
                table: "TenantModules",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_TenantId_SchoolId",
                schema: "billing",
                table: "Subscriptions",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_TenantId_SchoolId",
                schema: "schools",
                table: "StudentProfiles",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_Shifts_TenantId_SchoolId",
                schema: "academics",
                table: "Shifts",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_Scores_TenantId_SchoolId",
                schema: "academics",
                table: "Scores",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_SchoolRoles_TenantId_SchoolId",
                schema: "access",
                table: "SchoolRoles",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_SchoolReviews_TenantId_SchoolId",
                schema: "schools",
                table: "SchoolReviews",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_SchoolProfiles_TenantId_SchoolId",
                schema: "schools",
                table: "SchoolProfiles",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_SchoolPhotos_TenantId_SchoolId",
                schema: "schools",
                table: "SchoolPhotos",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_SchoolFacilities_TenantId_SchoolId",
                schema: "schools",
                table: "SchoolFacilities",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_SchoolEvents_TenantId_SchoolId",
                schema: "communication",
                table: "SchoolEvents",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_SchoolCurricula_TenantId_SchoolId",
                schema: "schools",
                table: "SchoolCurricula",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_TenantId_SchoolId",
                schema: "access",
                table: "RolePermissions",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_RenewalReminders_TenantId_SchoolId",
                schema: "billing",
                table: "RenewalReminders",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_Payments_TenantId_SchoolId",
                schema: "billing",
                table: "Payments",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_ParentStudents_TenantId_SchoolId",
                schema: "schools",
                table: "ParentStudents",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TenantId_SchoolId",
                schema: "communication",
                table: "Notifications",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_Invoices_TenantId_SchoolId",
                schema: "billing",
                table: "Invoices",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_TenantId_SchoolId",
                schema: "billing",
                table: "InvoiceLines",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_Holidays_TenantId_SchoolId",
                schema: "academics",
                table: "Holidays",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_GradingScales_TenantId_SchoolId",
                schema: "academics",
                table: "GradingScales",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_GradingBands_TenantId_SchoolId",
                schema: "academics",
                table: "GradingBands",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_TenantId_SchoolId",
                schema: "academics",
                table: "Enrolments",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_EnquiryFollowUps_TenantId_SchoolId",
                schema: "admissions",
                table: "EnquiryFollowUps",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_Courses_TenantId_SchoolId",
                schema: "academics",
                table: "Courses",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_ClassOfferings_TenantId_SchoolId",
                schema: "schools",
                table: "ClassOfferings",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_CampusVisits_TenantId_SchoolId",
                schema: "admissions",
                table: "CampusVisits",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_CampusPhones_TenantId_SchoolId",
                schema: "schools",
                table: "CampusPhones",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_Campuses_TenantId_SchoolId",
                schema: "schools",
                table: "Campuses",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvents_TenantId_SchoolId",
                schema: "academics",
                table: "CalendarEvents",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_BrandingImages_TenantId_SchoolId",
                schema: "schools",
                table: "BrandingImages",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_Assessments_TenantId_SchoolId",
                schema: "academics",
                table: "Assessments",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_AdmissionEnquiries_TenantId_SchoolId",
                schema: "admissions",
                table: "AdmissionEnquiries",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_AddOnRequests_TenantId_SchoolId",
                schema: "billing",
                table: "AddOnRequests",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_AccountRoles_TenantId_SchoolId",
                schema: "access",
                table: "AccountRoles",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.CreateIndex(
                name: "IX_AcademicYears_TenantId_SchoolId",
                schema: "academics",
                table: "AcademicYears",
                columns: new[] { "TenantId", "SchoolId" });
            migrationBuilder.AddForeignKey(
                name: "FK_AcademicYears_Tenants_TenantId",
                schema: "academics",
                table: "AcademicYears",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_AccountRoles_Tenants_TenantId",
                schema: "access",
                table: "AccountRoles",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_AddOnRequests_Tenants_TenantId",
                schema: "billing",
                table: "AddOnRequests",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_AdmissionEnquiries_Tenants_TenantId",
                schema: "admissions",
                table: "AdmissionEnquiries",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_Tenants_TenantId",
                schema: "academics",
                table: "Assessments",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_BrandingImages_Tenants_TenantId",
                schema: "schools",
                table: "BrandingImages",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_CalendarEvents_Tenants_TenantId",
                schema: "academics",
                table: "CalendarEvents",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Campuses_Tenants_TenantId",
                schema: "schools",
                table: "Campuses",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_CampusPhones_Tenants_TenantId",
                schema: "schools",
                table: "CampusPhones",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_CampusVisits_Tenants_TenantId",
                schema: "admissions",
                table: "CampusVisits",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_ClassOfferings_Tenants_TenantId",
                schema: "schools",
                table: "ClassOfferings",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Tenants_TenantId",
                schema: "academics",
                table: "Courses",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_EnquiryFollowUps_Tenants_TenantId",
                schema: "admissions",
                table: "EnquiryFollowUps",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Tenants_TenantId",
                schema: "academics",
                table: "Enrolments",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_GradingBands_Tenants_TenantId",
                schema: "academics",
                table: "GradingBands",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_GradingScales_Tenants_TenantId",
                schema: "academics",
                table: "GradingScales",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Holidays_Tenants_TenantId",
                schema: "academics",
                table: "Holidays",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLines_Tenants_TenantId",
                schema: "billing",
                table: "InvoiceLines",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Tenants_TenantId",
                schema: "billing",
                table: "Invoices",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Tenants_TenantId",
                schema: "communication",
                table: "Notifications",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_ParentStudents_Tenants_TenantId",
                schema: "schools",
                table: "ParentStudents",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Tenants_TenantId",
                schema: "billing",
                table: "Payments",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_RenewalReminders_Tenants_TenantId",
                schema: "billing",
                table: "RenewalReminders",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_RolePermissions_Tenants_TenantId",
                schema: "access",
                table: "RolePermissions",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_SchoolCurricula_Tenants_TenantId",
                schema: "schools",
                table: "SchoolCurricula",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_SchoolEvents_Tenants_TenantId",
                schema: "communication",
                table: "SchoolEvents",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_SchoolFacilities_Tenants_TenantId",
                schema: "schools",
                table: "SchoolFacilities",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_SchoolPhotos_Tenants_TenantId",
                schema: "schools",
                table: "SchoolPhotos",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_SchoolProfiles_Tenants_TenantId",
                schema: "schools",
                table: "SchoolProfiles",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_SchoolReviews_Tenants_TenantId",
                schema: "schools",
                table: "SchoolReviews",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_SchoolRoles_Tenants_TenantId",
                schema: "access",
                table: "SchoolRoles",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Scores_Tenants_TenantId",
                schema: "academics",
                table: "Scores",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Tenants_TenantId",
                schema: "academics",
                table: "Shifts",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_StudentProfiles_Tenants_TenantId",
                schema: "schools",
                table: "StudentProfiles",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Tenants_TenantId",
                schema: "billing",
                table: "Subscriptions",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_TenantModules_Tenants_TenantId",
                schema: "modules",
                table: "TenantModules",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_Terms_Tenants_TenantId",
                schema: "academics",
                table: "Terms",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_TimetableSlots_Tenants_TenantId",
                schema: "academics",
                table: "TimetableSlots",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_UserCampuses_Tenants_TenantId",
                schema: "access",
                table: "UserCampuses",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_UserDetails_Tenants_TenantId",
                schema: "schools",
                table: "UserDetails",
                column: "TenantId",
                principalSchema: "schools",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.Sql("""
                ALTER TABLE "schools"."UserDetails" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."UserDetails" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "schools"."UserDetails"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "schools"."UserDetails"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "access"."UserCampuses" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "access"."UserCampuses" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "access"."UserCampuses"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "access"."UserCampuses"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "academics"."TimetableSlots" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."TimetableSlots" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "academics"."TimetableSlots"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "academics"."TimetableSlots"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "academics"."Terms" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."Terms" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "academics"."Terms"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "academics"."Terms"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "modules"."TenantModules" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "modules"."TenantModules" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "modules"."TenantModules"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "modules"."TenantModules"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "billing"."Subscriptions" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "billing"."Subscriptions" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "billing"."Subscriptions"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "billing"."Subscriptions"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "schools"."StudentProfiles" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."StudentProfiles" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "schools"."StudentProfiles"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "schools"."StudentProfiles"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "academics"."Shifts" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."Shifts" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "academics"."Shifts"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "academics"."Shifts"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "academics"."Scores" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."Scores" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "academics"."Scores"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "academics"."Scores"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "access"."SchoolRoles" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "access"."SchoolRoles" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "access"."SchoolRoles"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "access"."SchoolRoles"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "schools"."SchoolReviews" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."SchoolReviews" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "schools"."SchoolReviews"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "schools"."SchoolReviews"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "schools"."SchoolProfiles" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."SchoolProfiles" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "schools"."SchoolProfiles"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "schools"."SchoolProfiles"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "schools"."SchoolPhotos" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."SchoolPhotos" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "schools"."SchoolPhotos"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "schools"."SchoolPhotos"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "schools"."SchoolFacilities" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."SchoolFacilities" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "schools"."SchoolFacilities"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "schools"."SchoolFacilities"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "communication"."SchoolEvents" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "communication"."SchoolEvents" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "communication"."SchoolEvents"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "communication"."SchoolEvents"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "schools"."SchoolCurricula" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."SchoolCurricula" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "schools"."SchoolCurricula"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "schools"."SchoolCurricula"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "access"."RolePermissions" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "access"."RolePermissions" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "access"."RolePermissions"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "access"."RolePermissions"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "billing"."RenewalReminders" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "billing"."RenewalReminders" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "billing"."RenewalReminders"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "billing"."RenewalReminders"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "billing"."Payments" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "billing"."Payments" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "billing"."Payments"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "billing"."Payments"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "schools"."ParentStudents" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."ParentStudents" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "schools"."ParentStudents"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "schools"."ParentStudents"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "communication"."Notifications" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "communication"."Notifications" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "communication"."Notifications"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "communication"."Notifications"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "billing"."Invoices" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "billing"."Invoices" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "billing"."Invoices"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "billing"."Invoices"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "billing"."InvoiceLines" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "billing"."InvoiceLines" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "billing"."InvoiceLines"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "billing"."InvoiceLines"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "academics"."Holidays" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."Holidays" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "academics"."Holidays"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "academics"."Holidays"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "academics"."GradingScales" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."GradingScales" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "academics"."GradingScales"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "academics"."GradingScales"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "academics"."GradingBands" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."GradingBands" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "academics"."GradingBands"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "academics"."GradingBands"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "academics"."Enrolments" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."Enrolments" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "academics"."Enrolments"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "academics"."Enrolments"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "admissions"."EnquiryFollowUps" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "admissions"."EnquiryFollowUps" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "admissions"."EnquiryFollowUps"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "admissions"."EnquiryFollowUps"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "academics"."Courses" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."Courses" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "academics"."Courses"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "academics"."Courses"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "schools"."ClassOfferings" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."ClassOfferings" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "schools"."ClassOfferings"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "schools"."ClassOfferings"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "admissions"."CampusVisits" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "admissions"."CampusVisits" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "admissions"."CampusVisits"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "admissions"."CampusVisits"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "schools"."CampusPhones" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."CampusPhones" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "schools"."CampusPhones"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "schools"."CampusPhones"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "schools"."Campuses" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."Campuses" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "schools"."Campuses"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "schools"."Campuses"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "academics"."CalendarEvents" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."CalendarEvents" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "academics"."CalendarEvents"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "academics"."CalendarEvents"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "schools"."BrandingImages" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."BrandingImages" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "schools"."BrandingImages"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "schools"."BrandingImages"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "academics"."Assessments" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."Assessments" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "academics"."Assessments"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "academics"."Assessments"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "admissions"."AdmissionEnquiries" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "admissions"."AdmissionEnquiries" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "admissions"."AdmissionEnquiries"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "admissions"."AdmissionEnquiries"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "billing"."AddOnRequests" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "billing"."AddOnRequests" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "billing"."AddOnRequests"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "billing"."AddOnRequests"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "access"."AccountRoles" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "access"."AccountRoles" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "access"."AccountRoles"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "access"."AccountRoles"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);
            migrationBuilder.Sql("""
                ALTER TABLE "academics"."AcademicYears" ENABLE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."AcademicYears" FORCE ROW LEVEL SECURITY;
                CREATE POLICY "TenantIsolation" ON "academics"."AcademicYears"
                    USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                    WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
                CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON "academics"."AcademicYears"
                    FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "schools"."UserDetails";
                DROP POLICY IF EXISTS "TenantIsolation" ON "schools"."UserDetails";
                ALTER TABLE "schools"."UserDetails" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."UserDetails" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "access"."UserCampuses";
                DROP POLICY IF EXISTS "TenantIsolation" ON "access"."UserCampuses";
                ALTER TABLE "access"."UserCampuses" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "access"."UserCampuses" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "academics"."TimetableSlots";
                DROP POLICY IF EXISTS "TenantIsolation" ON "academics"."TimetableSlots";
                ALTER TABLE "academics"."TimetableSlots" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."TimetableSlots" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "academics"."Terms";
                DROP POLICY IF EXISTS "TenantIsolation" ON "academics"."Terms";
                ALTER TABLE "academics"."Terms" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."Terms" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "modules"."TenantModules";
                DROP POLICY IF EXISTS "TenantIsolation" ON "modules"."TenantModules";
                ALTER TABLE "modules"."TenantModules" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "modules"."TenantModules" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "billing"."Subscriptions";
                DROP POLICY IF EXISTS "TenantIsolation" ON "billing"."Subscriptions";
                ALTER TABLE "billing"."Subscriptions" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "billing"."Subscriptions" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "schools"."StudentProfiles";
                DROP POLICY IF EXISTS "TenantIsolation" ON "schools"."StudentProfiles";
                ALTER TABLE "schools"."StudentProfiles" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."StudentProfiles" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "academics"."Shifts";
                DROP POLICY IF EXISTS "TenantIsolation" ON "academics"."Shifts";
                ALTER TABLE "academics"."Shifts" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."Shifts" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "academics"."Scores";
                DROP POLICY IF EXISTS "TenantIsolation" ON "academics"."Scores";
                ALTER TABLE "academics"."Scores" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."Scores" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "access"."SchoolRoles";
                DROP POLICY IF EXISTS "TenantIsolation" ON "access"."SchoolRoles";
                ALTER TABLE "access"."SchoolRoles" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "access"."SchoolRoles" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "schools"."SchoolReviews";
                DROP POLICY IF EXISTS "TenantIsolation" ON "schools"."SchoolReviews";
                ALTER TABLE "schools"."SchoolReviews" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."SchoolReviews" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "schools"."SchoolProfiles";
                DROP POLICY IF EXISTS "TenantIsolation" ON "schools"."SchoolProfiles";
                ALTER TABLE "schools"."SchoolProfiles" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."SchoolProfiles" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "schools"."SchoolPhotos";
                DROP POLICY IF EXISTS "TenantIsolation" ON "schools"."SchoolPhotos";
                ALTER TABLE "schools"."SchoolPhotos" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."SchoolPhotos" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "schools"."SchoolFacilities";
                DROP POLICY IF EXISTS "TenantIsolation" ON "schools"."SchoolFacilities";
                ALTER TABLE "schools"."SchoolFacilities" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."SchoolFacilities" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "communication"."SchoolEvents";
                DROP POLICY IF EXISTS "TenantIsolation" ON "communication"."SchoolEvents";
                ALTER TABLE "communication"."SchoolEvents" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "communication"."SchoolEvents" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "schools"."SchoolCurricula";
                DROP POLICY IF EXISTS "TenantIsolation" ON "schools"."SchoolCurricula";
                ALTER TABLE "schools"."SchoolCurricula" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."SchoolCurricula" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "access"."RolePermissions";
                DROP POLICY IF EXISTS "TenantIsolation" ON "access"."RolePermissions";
                ALTER TABLE "access"."RolePermissions" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "access"."RolePermissions" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "billing"."RenewalReminders";
                DROP POLICY IF EXISTS "TenantIsolation" ON "billing"."RenewalReminders";
                ALTER TABLE "billing"."RenewalReminders" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "billing"."RenewalReminders" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "billing"."Payments";
                DROP POLICY IF EXISTS "TenantIsolation" ON "billing"."Payments";
                ALTER TABLE "billing"."Payments" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "billing"."Payments" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "schools"."ParentStudents";
                DROP POLICY IF EXISTS "TenantIsolation" ON "schools"."ParentStudents";
                ALTER TABLE "schools"."ParentStudents" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."ParentStudents" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "communication"."Notifications";
                DROP POLICY IF EXISTS "TenantIsolation" ON "communication"."Notifications";
                ALTER TABLE "communication"."Notifications" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "communication"."Notifications" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "billing"."Invoices";
                DROP POLICY IF EXISTS "TenantIsolation" ON "billing"."Invoices";
                ALTER TABLE "billing"."Invoices" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "billing"."Invoices" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "billing"."InvoiceLines";
                DROP POLICY IF EXISTS "TenantIsolation" ON "billing"."InvoiceLines";
                ALTER TABLE "billing"."InvoiceLines" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "billing"."InvoiceLines" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "academics"."Holidays";
                DROP POLICY IF EXISTS "TenantIsolation" ON "academics"."Holidays";
                ALTER TABLE "academics"."Holidays" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."Holidays" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "academics"."GradingScales";
                DROP POLICY IF EXISTS "TenantIsolation" ON "academics"."GradingScales";
                ALTER TABLE "academics"."GradingScales" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."GradingScales" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "academics"."GradingBands";
                DROP POLICY IF EXISTS "TenantIsolation" ON "academics"."GradingBands";
                ALTER TABLE "academics"."GradingBands" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."GradingBands" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "academics"."Enrolments";
                DROP POLICY IF EXISTS "TenantIsolation" ON "academics"."Enrolments";
                ALTER TABLE "academics"."Enrolments" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."Enrolments" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "admissions"."EnquiryFollowUps";
                DROP POLICY IF EXISTS "TenantIsolation" ON "admissions"."EnquiryFollowUps";
                ALTER TABLE "admissions"."EnquiryFollowUps" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "admissions"."EnquiryFollowUps" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "academics"."Courses";
                DROP POLICY IF EXISTS "TenantIsolation" ON "academics"."Courses";
                ALTER TABLE "academics"."Courses" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."Courses" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "schools"."ClassOfferings";
                DROP POLICY IF EXISTS "TenantIsolation" ON "schools"."ClassOfferings";
                ALTER TABLE "schools"."ClassOfferings" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."ClassOfferings" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "admissions"."CampusVisits";
                DROP POLICY IF EXISTS "TenantIsolation" ON "admissions"."CampusVisits";
                ALTER TABLE "admissions"."CampusVisits" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "admissions"."CampusVisits" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "schools"."CampusPhones";
                DROP POLICY IF EXISTS "TenantIsolation" ON "schools"."CampusPhones";
                ALTER TABLE "schools"."CampusPhones" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."CampusPhones" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "schools"."Campuses";
                DROP POLICY IF EXISTS "TenantIsolation" ON "schools"."Campuses";
                ALTER TABLE "schools"."Campuses" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."Campuses" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "academics"."CalendarEvents";
                DROP POLICY IF EXISTS "TenantIsolation" ON "academics"."CalendarEvents";
                ALTER TABLE "academics"."CalendarEvents" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."CalendarEvents" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "schools"."BrandingImages";
                DROP POLICY IF EXISTS "TenantIsolation" ON "schools"."BrandingImages";
                ALTER TABLE "schools"."BrandingImages" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "schools"."BrandingImages" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "academics"."Assessments";
                DROP POLICY IF EXISTS "TenantIsolation" ON "academics"."Assessments";
                ALTER TABLE "academics"."Assessments" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."Assessments" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "admissions"."AdmissionEnquiries";
                DROP POLICY IF EXISTS "TenantIsolation" ON "admissions"."AdmissionEnquiries";
                ALTER TABLE "admissions"."AdmissionEnquiries" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "admissions"."AdmissionEnquiries" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "billing"."AddOnRequests";
                DROP POLICY IF EXISTS "TenantIsolation" ON "billing"."AddOnRequests";
                ALTER TABLE "billing"."AddOnRequests" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "billing"."AddOnRequests" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "access"."AccountRoles";
                DROP POLICY IF EXISTS "TenantIsolation" ON "access"."AccountRoles";
                ALTER TABLE "access"."AccountRoles" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "access"."AccountRoles" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS "TenantFromSchool" ON "academics"."AcademicYears";
                DROP POLICY IF EXISTS "TenantIsolation" ON "academics"."AcademicYears";
                ALTER TABLE "academics"."AcademicYears" NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE "academics"."AcademicYears" DISABLE ROW LEVEL SECURITY;
                """);
            migrationBuilder.Sql("""
                DROP FUNCTION IF EXISTS public.education_portal_set_tenant();
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_AcademicYears_Tenants_TenantId",
                schema: "academics",
                table: "AcademicYears");

            migrationBuilder.DropForeignKey(
                name: "FK_AccountRoles_Tenants_TenantId",
                schema: "access",
                table: "AccountRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AddOnRequests_Tenants_TenantId",
                schema: "billing",
                table: "AddOnRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_AdmissionEnquiries_Tenants_TenantId",
                schema: "admissions",
                table: "AdmissionEnquiries");

            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_Tenants_TenantId",
                schema: "academics",
                table: "Assessments");

            migrationBuilder.DropForeignKey(
                name: "FK_BrandingImages_Tenants_TenantId",
                schema: "schools",
                table: "BrandingImages");

            migrationBuilder.DropForeignKey(
                name: "FK_CalendarEvents_Tenants_TenantId",
                schema: "academics",
                table: "CalendarEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Campuses_Tenants_TenantId",
                schema: "schools",
                table: "Campuses");

            migrationBuilder.DropForeignKey(
                name: "FK_CampusPhones_Tenants_TenantId",
                schema: "schools",
                table: "CampusPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_CampusVisits_Tenants_TenantId",
                schema: "admissions",
                table: "CampusVisits");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassOfferings_Tenants_TenantId",
                schema: "schools",
                table: "ClassOfferings");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Tenants_TenantId",
                schema: "academics",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_EnquiryFollowUps_Tenants_TenantId",
                schema: "admissions",
                table: "EnquiryFollowUps");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Tenants_TenantId",
                schema: "academics",
                table: "Enrolments");

            migrationBuilder.DropForeignKey(
                name: "FK_GradingBands_Tenants_TenantId",
                schema: "academics",
                table: "GradingBands");

            migrationBuilder.DropForeignKey(
                name: "FK_GradingScales_Tenants_TenantId",
                schema: "academics",
                table: "GradingScales");

            migrationBuilder.DropForeignKey(
                name: "FK_Holidays_Tenants_TenantId",
                schema: "academics",
                table: "Holidays");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLines_Tenants_TenantId",
                schema: "billing",
                table: "InvoiceLines");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Tenants_TenantId",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Tenants_TenantId",
                schema: "communication",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_ParentStudents_Tenants_TenantId",
                schema: "schools",
                table: "ParentStudents");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Tenants_TenantId",
                schema: "billing",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_RenewalReminders_Tenants_TenantId",
                schema: "billing",
                table: "RenewalReminders");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermissions_Tenants_TenantId",
                schema: "access",
                table: "RolePermissions");

            migrationBuilder.DropForeignKey(
                name: "FK_SchoolCurricula_Tenants_TenantId",
                schema: "schools",
                table: "SchoolCurricula");

            migrationBuilder.DropForeignKey(
                name: "FK_SchoolEvents_Tenants_TenantId",
                schema: "communication",
                table: "SchoolEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_SchoolFacilities_Tenants_TenantId",
                schema: "schools",
                table: "SchoolFacilities");

            migrationBuilder.DropForeignKey(
                name: "FK_SchoolPhotos_Tenants_TenantId",
                schema: "schools",
                table: "SchoolPhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_SchoolProfiles_Tenants_TenantId",
                schema: "schools",
                table: "SchoolProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_SchoolReviews_Tenants_TenantId",
                schema: "schools",
                table: "SchoolReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_SchoolRoles_Tenants_TenantId",
                schema: "access",
                table: "SchoolRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_Scores_Tenants_TenantId",
                schema: "academics",
                table: "Scores");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Tenants_TenantId",
                schema: "academics",
                table: "Shifts");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentProfiles_Tenants_TenantId",
                schema: "schools",
                table: "StudentProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Tenants_TenantId",
                schema: "billing",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantModules_Tenants_TenantId",
                schema: "modules",
                table: "TenantModules");

            migrationBuilder.DropForeignKey(
                name: "FK_Terms_Tenants_TenantId",
                schema: "academics",
                table: "Terms");

            migrationBuilder.DropForeignKey(
                name: "FK_TimetableSlots_Tenants_TenantId",
                schema: "academics",
                table: "TimetableSlots");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCampuses_Tenants_TenantId",
                schema: "access",
                table: "UserCampuses");

            migrationBuilder.DropForeignKey(
                name: "FK_UserDetails_Tenants_TenantId",
                schema: "schools",
                table: "UserDetails");

            migrationBuilder.DropIndex(
                name: "IX_UserDetails_TenantId_SchoolId",
                schema: "schools",
                table: "UserDetails");

            migrationBuilder.DropIndex(
                name: "IX_UserCampuses_TenantId_SchoolId",
                schema: "access",
                table: "UserCampuses");

            migrationBuilder.DropIndex(
                name: "IX_TimetableSlots_TenantId_SchoolId",
                schema: "academics",
                table: "TimetableSlots");

            migrationBuilder.DropIndex(
                name: "IX_Terms_TenantId_SchoolId",
                schema: "academics",
                table: "Terms");

            migrationBuilder.DropIndex(
                name: "IX_TenantModules_TenantId_SchoolId",
                schema: "modules",
                table: "TenantModules");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_TenantId_SchoolId",
                schema: "billing",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_StudentProfiles_TenantId_SchoolId",
                schema: "schools",
                table: "StudentProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_TenantId_SchoolId",
                schema: "academics",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Scores_TenantId_SchoolId",
                schema: "academics",
                table: "Scores");

            migrationBuilder.DropIndex(
                name: "IX_SchoolRoles_TenantId_SchoolId",
                schema: "access",
                table: "SchoolRoles");

            migrationBuilder.DropIndex(
                name: "IX_SchoolReviews_TenantId_SchoolId",
                schema: "schools",
                table: "SchoolReviews");

            migrationBuilder.DropIndex(
                name: "IX_SchoolProfiles_TenantId_SchoolId",
                schema: "schools",
                table: "SchoolProfiles");

            migrationBuilder.DropIndex(
                name: "IX_SchoolPhotos_TenantId_SchoolId",
                schema: "schools",
                table: "SchoolPhotos");

            migrationBuilder.DropIndex(
                name: "IX_SchoolFacilities_TenantId_SchoolId",
                schema: "schools",
                table: "SchoolFacilities");

            migrationBuilder.DropIndex(
                name: "IX_SchoolEvents_TenantId_SchoolId",
                schema: "communication",
                table: "SchoolEvents");

            migrationBuilder.DropIndex(
                name: "IX_SchoolCurricula_TenantId_SchoolId",
                schema: "schools",
                table: "SchoolCurricula");

            migrationBuilder.DropIndex(
                name: "IX_RolePermissions_TenantId_SchoolId",
                schema: "access",
                table: "RolePermissions");

            migrationBuilder.DropIndex(
                name: "IX_RenewalReminders_TenantId_SchoolId",
                schema: "billing",
                table: "RenewalReminders");

            migrationBuilder.DropIndex(
                name: "IX_Payments_TenantId_SchoolId",
                schema: "billing",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_ParentStudents_TenantId_SchoolId",
                schema: "schools",
                table: "ParentStudents");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_TenantId_SchoolId",
                schema: "communication",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_TenantId_SchoolId",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceLines_TenantId_SchoolId",
                schema: "billing",
                table: "InvoiceLines");

            migrationBuilder.DropIndex(
                name: "IX_Holidays_TenantId_SchoolId",
                schema: "academics",
                table: "Holidays");

            migrationBuilder.DropIndex(
                name: "IX_GradingScales_TenantId_SchoolId",
                schema: "academics",
                table: "GradingScales");

            migrationBuilder.DropIndex(
                name: "IX_GradingBands_TenantId_SchoolId",
                schema: "academics",
                table: "GradingBands");

            migrationBuilder.DropIndex(
                name: "IX_Enrolments_TenantId_SchoolId",
                schema: "academics",
                table: "Enrolments");

            migrationBuilder.DropIndex(
                name: "IX_EnquiryFollowUps_TenantId_SchoolId",
                schema: "admissions",
                table: "EnquiryFollowUps");

            migrationBuilder.DropIndex(
                name: "IX_Courses_TenantId_SchoolId",
                schema: "academics",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_ClassOfferings_TenantId_SchoolId",
                schema: "schools",
                table: "ClassOfferings");

            migrationBuilder.DropIndex(
                name: "IX_CampusVisits_TenantId_SchoolId",
                schema: "admissions",
                table: "CampusVisits");

            migrationBuilder.DropIndex(
                name: "IX_CampusPhones_TenantId_SchoolId",
                schema: "schools",
                table: "CampusPhones");

            migrationBuilder.DropIndex(
                name: "IX_Campuses_TenantId_SchoolId",
                schema: "schools",
                table: "Campuses");

            migrationBuilder.DropIndex(
                name: "IX_CalendarEvents_TenantId_SchoolId",
                schema: "academics",
                table: "CalendarEvents");

            migrationBuilder.DropIndex(
                name: "IX_BrandingImages_TenantId_SchoolId",
                schema: "schools",
                table: "BrandingImages");

            migrationBuilder.DropIndex(
                name: "IX_Assessments_TenantId_SchoolId",
                schema: "academics",
                table: "Assessments");

            migrationBuilder.DropIndex(
                name: "IX_AdmissionEnquiries_TenantId_SchoolId",
                schema: "admissions",
                table: "AdmissionEnquiries");

            migrationBuilder.DropIndex(
                name: "IX_AddOnRequests_TenantId_SchoolId",
                schema: "billing",
                table: "AddOnRequests");

            migrationBuilder.DropIndex(
                name: "IX_AccountRoles_TenantId_SchoolId",
                schema: "access",
                table: "AccountRoles");

            migrationBuilder.DropIndex(
                name: "IX_AcademicYears_TenantId_SchoolId",
                schema: "academics",
                table: "AcademicYears");

            migrationBuilder.DeleteData(
                schema: "lookups",
                table: "SecurityAuditEventTypes",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "schools",
                table: "UserDetails");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "access",
                table: "UserCampuses");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "academics",
                table: "TimetableSlots");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "academics",
                table: "Terms");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "modules",
                table: "TenantModules");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "billing",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "schools",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "academics",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "academics",
                table: "Scores");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "access",
                table: "SchoolRoles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "schools",
                table: "SchoolReviews");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "schools",
                table: "SchoolProfiles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "schools",
                table: "SchoolPhotos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "schools",
                table: "SchoolFacilities");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "communication",
                table: "SchoolEvents");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "schools",
                table: "SchoolCurricula");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "access",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "billing",
                table: "RenewalReminders");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "billing",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "schools",
                table: "ParentStudents");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "communication",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "billing",
                table: "InvoiceLines");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "academics",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "academics",
                table: "GradingScales");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "academics",
                table: "GradingBands");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "academics",
                table: "Enrolments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "admissions",
                table: "EnquiryFollowUps");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "academics",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "schools",
                table: "ClassOfferings");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "admissions",
                table: "CampusVisits");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "schools",
                table: "CampusPhones");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "schools",
                table: "Campuses");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "academics",
                table: "CalendarEvents");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "schools",
                table: "BrandingImages");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "academics",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "admissions",
                table: "AdmissionEnquiries");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "billing",
                table: "AddOnRequests");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "access",
                table: "AccountRoles");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "academics",
                table: "AcademicYears");
        }
    }
}

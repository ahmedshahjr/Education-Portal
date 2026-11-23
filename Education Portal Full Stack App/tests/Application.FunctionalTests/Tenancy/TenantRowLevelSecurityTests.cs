using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Common;
using EducationPortal.Domain.Entities;
using EducationPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.Tenancy;

/// <summary>
/// The database's own tenant isolation (ADR-026), tested without the application. The tests connect as a role that is not
/// a superuser and does not bypass row-level security, because PostgreSQL never applies row-level security to a superuser.
/// The development database is owned by a superuser, so the application's connection is not checked here by itself; the
/// role below is the same kind of connection a production deployment must use.
/// </summary>
public class TenantRowLevelSecurityTests : TestBase
{
    private const string AppRole = "education_portal_rls_test";
    private const string AppRolePassword = "rls-test-only-6f1c2b9e7a0d4c58";
    private const string ProbeSchema = "rls_probe";
    private const string ProbeTable = "probe";

    [Test]
    public async Task ShouldShowATenantOnlyItsOwnRows()
    {
        var (one, two) = await TwoSchoolsAsync();
        var oneCampuses = await CampusCountAsync(one.Id);
        var twoCampuses = await CampusCountAsync(two.Id);

        await using var connection = await OpenAsAppAsync(TenantOf(one));
        var visibleSchools = await DistinctSchoolsAsync(connection);

        visibleSchools.ShouldBe(new[] { one.Id });
        oneCampuses.ShouldBeGreaterThan(0);
        twoCampuses.ShouldBeGreaterThan(0);
        (await CountCampusesAsync(connection)).ShouldBe(oneCampuses);
    }

    [Test]
    public async Task ShouldShowNoTenantRowsWithoutATenant()
    {
        await TwoSchoolsAsync();

        await using var connection = await OpenAsAppAsync(tenantId: null);

        (await CountCampusesAsync(connection)).ShouldBe(0);
    }

    [Test]
    public async Task ShouldShowEveryTenantsRowsOnlyWithTheBypass()
    {
        var (one, two) = await TwoSchoolsAsync();
        var everything = await CampusCountAsync(null);

        await using var asTenant = await OpenAsAppAsync(TenantOf(one));
        (await CountCampusesAsync(asTenant)).ShouldBeLessThan(everything);

        await using var asMaster = await OpenAsAppAsync(TenantOf(one), bypass: true);
        (await CountCampusesAsync(asMaster)).ShouldBe(everything);
        var visible = await DistinctSchoolsAsync(asMaster);
        visible.ShouldContain(one.Id);
        visible.ShouldContain(two.Id);
    }

    [Test]
    public async Task ShouldRefuseAnInsertIntoAnotherTenantsSchool()
    {
        var (one, two) = await TwoSchoolsAsync();
        await EnsureProbeAsync();

        await using var connection = await OpenAsAppAsync(TenantOf(one));

        // The trigger fills the tenant from the school (school two), and the policy then refuses the row for tenant one.
        var error = await Should.ThrowAsync<PostgresException>(() => InsertProbeAsync(connection, two.Id, tenantId: TenantOf(one)));
        error.SqlState.ShouldBe("42501");
    }

    [Test]
    public async Task ShouldKeepTheSchoolsTenantWhateverTheInsertSays()
    {
        var (one, two) = await TwoSchoolsAsync();
        await EnsureProbeAsync();

        await using var connection = await OpenAsAppAsync(TenantOf(one));

        // The row claims tenant two, but the database writes the tenant of its school, tenant one.
        await InsertProbeAsync(connection, one.Id, tenantId: TenantOf(two));

        (await ProbeTenantsAsync(connection)).ShouldBe(new[] { TenantOf(one) });
    }

    [Test]
    public async Task ShouldRefuseMovingARowToAnotherSchool()
    {
        var (one, two) = await TwoSchoolsAsync();
        await EnsureProbeAsync();
        await using (var seed = await OpenAsAppAsync(TenantOf(one)))
        {
            await InsertProbeAsync(seed, one.Id, tenantId: TenantOf(one));
        }

        await using var connection = await OpenAsAppAsync(TenantOf(one));

        var error = await Should.ThrowAsync<PostgresException>(() => ExecuteAsync(
            connection,
            $"UPDATE {ProbeSchema}.{ProbeTable} SET \"SchoolId\" = {two.Id}"));
        error.SqlState.ShouldBe("23514");
    }

    [Test]
    public async Task ShouldCoverEveryTenantOwnedTableWithRowLevelSecurityAndTheTrigger()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var model = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().GetService<Microsoft.EntityFrameworkCore.Metadata.IDesignTimeModel>().Model;
        var expected = model.GetEntityTypes()
            .Where(t => typeof(ITenantOwned).IsAssignableFrom(t.ClrType))
            .Select(t => $"{t.GetSchema()}.{t.GetTableName()}")
            .ToList();

        await using var connection = new NpgsqlConnection(FunctionalTestSetup.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            SELECT n.nspname || '.' || c.relname,
                   c.relrowsecurity AND c.relforcerowsecurity,
                   EXISTS (SELECT 1 FROM pg_policy p WHERE p.polrelid = c.oid AND p.polname = 'TenantIsolation'),
                   EXISTS (SELECT 1 FROM pg_trigger t WHERE t.tgrelid = c.oid AND t.tgname = 'TenantFromSchool' AND NOT t.tgisinternal)
            FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
            """,
            connection);
        await using var reader = await command.ExecuteReaderAsync();

        var covered = new Dictionary<string, (bool Forced, bool Policy, bool Trigger)>();
        while (await reader.ReadAsync())
        {
            covered[reader.GetString(0)] = (reader.GetBoolean(1), reader.GetBoolean(2), reader.GetBoolean(3));
        }

        var missing = expected
            .Where(table => !covered.TryGetValue(table, out var state) || !(state.Forced && state.Policy && state.Trigger))
            .ToList();

        missing.ShouldBeEmpty("These tenant-owned tables lack forced row-level security, the TenantIsolation policy or the TenantFromSchool trigger");
    }

    /// <summary>Two schools of two tenants, each with its main campus, plus the master tenant's own school.</summary>
    private static async Task<(EntityIds One, EntityIds Two)> TwoSchoolsAsync()
    {
        var one = await TestApp.AddSchoolAsync("RLS School One", "rls-one.localhost");
        var two = await TestApp.AddSchoolAsync("RLS School Two", "rls-two.localhost");
        return (one, two);
    }

    private static int TenantOf(EntityIds school) => TestApp.Resolve(school.Id).TenantId;

    private static async Task<int> CampusCountAsync(int? schoolId)
    {
        await using var connection = new NpgsqlConnection(FunctionalTestSetup.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            schoolId is null
                ? "SELECT count(*) FROM schools.\"Campuses\""
                : $"SELECT count(*) FROM schools.\"Campuses\" WHERE \"SchoolId\" = {schoolId.Value}",
            connection);
        return Convert.ToInt32(await command.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture);
    }

    private static async Task<int> CountCampusesAsync(NpgsqlConnection connection)
    {
        await using var command = new NpgsqlCommand("SELECT count(*) FROM schools.\"Campuses\"", connection);
        return Convert.ToInt32(await command.ExecuteScalarAsync(), System.Globalization.CultureInfo.InvariantCulture);
    }

    private static async Task<int[]> DistinctSchoolsAsync(NpgsqlConnection connection)
    {
        await using var command = new NpgsqlCommand("SELECT DISTINCT \"SchoolId\" FROM schools.\"Campuses\" ORDER BY 1", connection);
        await using var reader = await command.ExecuteReaderAsync();
        var schools = new List<int>();
        while (await reader.ReadAsync())
        {
            schools.Add(reader.GetInt32(0));
        }

        return schools.ToArray();
    }

    /// <summary>Opens a connection as the non-superuser role, with the session settings the application's interceptor sets.</summary>
    private static async Task<NpgsqlConnection> OpenAsAppAsync(int? tenantId, bool bypass = false)
    {
        await EnsureAppRoleAsync();

        var builder = new NpgsqlConnectionStringBuilder(FunctionalTestSetup.ConnectionString)
        {
            Username = AppRole,
            Password = AppRolePassword,
        };
        var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "SELECT set_config('app.tenant_id', @tenant, false), set_config('app.tenant_bypass', @bypass, false)",
            connection);
        command.Parameters.AddWithValue("tenant", tenantId?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty);
        command.Parameters.AddWithValue("bypass", bypass ? "on" : "off");
        await command.ExecuteNonQueryAsync();

        return connection;
    }

    /// <summary>Creates the test role once per database. It is not a superuser and does not bypass row-level security.</summary>
    private static async Task EnsureAppRoleAsync()
    {
        await using var connection = new NpgsqlConnection(FunctionalTestSetup.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            $"""
            DO $$
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = '{AppRole}') THEN
                    CREATE ROLE {AppRole} LOGIN PASSWORD '{AppRolePassword}' NOSUPERUSER NOBYPASSRLS NOCREATEDB NOCREATEROLE;
                END IF;
            END $$;
            GRANT USAGE ON SCHEMA schools, access, billing, academics, admissions, communication, modules TO {AppRole};
            GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA schools, access, billing, academics, admissions, communication, modules TO {AppRole};
            GRANT USAGE ON ALL SEQUENCES IN SCHEMA schools, access, billing, academics, admissions, communication, modules TO {AppRole};
            """,
            connection);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// A probe table with the same trigger and policy as the real tables, so the mechanism is tested on its own. The coverage
    /// test checks the real tables.
    /// </summary>
    private static async Task EnsureProbeAsync()
    {
        await EnsureAppRoleAsync();
        await using var connection = new NpgsqlConnection(FunctionalTestSetup.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            $"""
            CREATE SCHEMA IF NOT EXISTS {ProbeSchema};
            GRANT USAGE ON SCHEMA {ProbeSchema} TO {AppRole};
            DROP TABLE IF EXISTS {ProbeSchema}.{ProbeTable};
            CREATE TABLE {ProbeSchema}.{ProbeTable} (
                "Id" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                "SchoolId" integer NOT NULL,
                "TenantId" integer NOT NULL
            );
            ALTER TABLE {ProbeSchema}.{ProbeTable} ENABLE ROW LEVEL SECURITY;
            ALTER TABLE {ProbeSchema}.{ProbeTable} FORCE ROW LEVEL SECURITY;
            CREATE POLICY "TenantIsolation" ON {ProbeSchema}.{ProbeTable}
                USING ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on')
                WITH CHECK ("TenantId" = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on');
            CREATE TRIGGER "TenantFromSchool" BEFORE INSERT OR UPDATE ON {ProbeSchema}.{ProbeTable}
                FOR EACH ROW EXECUTE FUNCTION public.education_portal_set_tenant();
            GRANT SELECT, INSERT, UPDATE, DELETE ON {ProbeSchema}.{ProbeTable} TO {AppRole};
            GRANT USAGE ON ALL SEQUENCES IN SCHEMA {ProbeSchema} TO {AppRole};
            """,
            connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task InsertProbeAsync(NpgsqlConnection connection, int schoolId, int tenantId)
    {
        await ExecuteAsync(
            connection,
            $"INSERT INTO {ProbeSchema}.{ProbeTable} (\"SchoolId\", \"TenantId\") VALUES ({schoolId}, {tenantId})");
    }

    private static async Task<int[]> ProbeTenantsAsync(NpgsqlConnection connection)
    {
        await using var command = new NpgsqlCommand($"SELECT DISTINCT \"TenantId\" FROM {ProbeSchema}.{ProbeTable} ORDER BY 1", connection);
        await using var reader = await command.ExecuteReaderAsync();
        var tenants = new List<int>();
        while (await reader.ReadAsync())
        {
            tenants.Add(reader.GetInt32(0));
        }

        return tenants.ToArray();
    }

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}

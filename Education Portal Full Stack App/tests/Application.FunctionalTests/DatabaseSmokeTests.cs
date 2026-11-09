using EducationPortal.Infrastructure.Identity;

namespace EducationPortal.Application.FunctionalTests;

/// <summary>
/// Proves the functional test setup works end to end: Aspire starts PostgreSQL,
/// the Web host creates the schema, and Respawn resets the data between tests.
/// </summary>
public class DatabaseSmokeTests : TestBase
{
    [Test]
    public async Task ShouldStoreUsersInPostgreSql()
    {
        var userId = await TestApp.RunAsAdministratorAsync();

        var user = await TestApp.FindAsync<ApplicationUser>(userId);

        user.ShouldNotBeNull();
        user.UserName.ShouldBe("administrator@local");
    }

    [Test]
    public async Task ShouldStartEachTestWithAnEmptyDatabase()
    {
        var count = await TestApp.CountAsync<ApplicationUser>();

        count.ShouldBe(0);
    }
}

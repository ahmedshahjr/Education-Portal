using System.Net;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Domain.Entities;
using EducationPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.Schools;

/// <summary>Editing a school's name and domain (master tenant only): the domain must be unused, and the master's never changes.</summary>
public class SchoolProfileTests : TestBase
{
    [Test]
    public async Task ShouldChangeASchoolsNameAndItsDomain()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await TestApp.AddSchoolAsync("School One", "school1.localhost");

        using var response = await master.PutAsync($"/api/Schools/{school.PublicId}", new { name = "School Uno", domain = "Uno.localhost" });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var saved = await LoadSchoolAsync(school.Id);
        saved.Name.ShouldBe("School Uno");
        saved.Domain.ShouldBe("uno.localhost");
    }

    [Test]
    public async Task ShouldRefuseADomainAnotherSchoolUses()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        await TestApp.AddSchoolAsync("School Two", "school2.localhost");
        var school = await TestApp.AddSchoolAsync("School One", "school1.localhost");

        using var response = await master.PutAsync($"/api/Schools/{school.PublicId}", new { name = "School One", domain = "school2.localhost" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await LoadSchoolAsync(school.Id)).Domain.ShouldBe("school1.localhost");
    }

    [Test]
    public async Task ShouldNeverChangeTheMasterTenantsDomain()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var masterSchool = await LoadMasterAsync();

        using var response = await master.PutAsync($"/api/Schools/{masterSchool.PublicId}", new { name = "Platform", domain = "elsewhere.localhost" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await LoadSchoolAsync(masterSchool.Id)).Domain.ShouldBe(masterSchool.Domain);
    }

    [Test]
    public async Task ShouldRefuseABlankName()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await TestApp.AddSchoolAsync("School One", "school1.localhost");

        using var response = await master.PutAsync($"/api/Schools/{school.PublicId}", new { name = " ", domain = "school1.localhost" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await LoadSchoolAsync(school.Id)).Name.ShouldBe("School One");
    }

    private static async Task<HttpSession> SignInAsMasterAsync()
    {
        var master = TestHttp.CreateDevelopmentSession();
        using var response = await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return master;
    }

    private static async Task<School> LoadSchoolAsync(int id)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Schools.AsNoTracking().SingleAsync(s => s.Id == id);
    }

    private static async Task<School> LoadMasterAsync()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Schools.AsNoTracking().SingleAsync(s => s.IsMaster);
    }
}

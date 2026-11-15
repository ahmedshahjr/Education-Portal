using System.Net;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Domain.Entities;
using EducationPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.Schools;

/// <summary>Deleting a school (master tenant only): the school is soft-deleted, and the master tenant itself cannot be deleted.</summary>
public class SchoolDeleteTests : TestBase
{
    [Test]
    public async Task ShouldDeleteASchoolAndHideItFromTheList()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await TestApp.AddSchoolAsync("School One", "school1.localhost");

        using var response = await master.DeleteAsync($"/api/Schools/{school.PublicId}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        using var list = await master.GetAsync("/api/Schools?pageNumber=1&pageSize=100");
        (await list.Content.ReadAsStringAsync()).ShouldNotContain(school.PublicId.ToString());
    }

    [Test]
    public async Task ShouldRefuseToDeleteTheMasterTenant()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var masterSchool = await LoadMasterAsync();

        using var response = await master.DeleteAsync($"/api/Schools/{masterSchool.PublicId}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private static async Task<HttpSession> SignInAsMasterAsync()
    {
        var master = TestHttp.CreateDevelopmentSession();
        using var response = await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return master;
    }

    private static async Task<School> LoadMasterAsync()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Schools.AsNoTracking().SingleAsync(s => s.IsMaster);
    }
}

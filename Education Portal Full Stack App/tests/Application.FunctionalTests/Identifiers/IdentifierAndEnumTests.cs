using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using EducationPortal.Infrastructure.Data.Lookups;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace EducationPortal.Application.FunctionalTests.Identifiers;

/// <summary>
/// Keys and enums across the layers (identifiers-enums skill): the API shows only Guid public ids and refuses the internal
/// long key (ENG-ID-02, ENG-ID-03); enums travel by name only (ENG-ENUM-01), are stored as numbers (ENG-ENUM-03) and have
/// a seeded lookup table that refuses an undefined number (ENG-ENUM-04).
/// </summary>
public class IdentifierAndEnumTests : TestBase
{
    private const string SchoolDomain = "school1.localhost";

    [Test]
    public async Task ShouldShowOnlyPublicIdsInResponses()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var response = await master.GetAsync("/api/Schools");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        var ids = json.RootElement.GetProperty("items").EnumerateArray().Select(s => s.GetProperty("id")).ToList();
        ids.ShouldAllBe(id => id.ValueKind == JsonValueKind.String);
        ids.Select(id => id.GetGuid()).ShouldContain(school.PublicId);
        ids.Select(id => id.GetGuid()).ShouldContain(TestApp.MasterSchoolPublicId);
    }

    [Test]
    public async Task ShouldRefuseTheInternalKeyInARoute()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var byLong = await master.GetAsync($"/api/Schools/{school.Id}/branding");
        using var byGuid = await master.GetAsync($"/api/Schools/{school.PublicId}/branding");
        using var unknown = await master.GetAsync($"/api/Schools/{Guid.CreateVersion7()}/branding");

        byLong.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        byGuid.StatusCode.ShouldBe(HttpStatusCode.OK);
        unknown.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ShouldAcceptEnumsByNameOnly()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var byName = await master.PutAsync($"/api/Modules/{ModuleKeys.Parents}", new { type = "Default", isActive = true });
        using var byNumber = await master.PutAsync($"/api/Modules/{ModuleKeys.Parents}", new { type = 1, isActive = true });
        using var undefined = await master.PutAsync($"/api/Modules/{ModuleKeys.Parents}", new { type = "Optional", isActive = true });

        byName.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        byNumber.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        undefined.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRefuseAnEnumNumberInARoute()
    {
        using var session = TestHttp.CreateDevelopmentSession();

        using var byNumber = await session.GetAsync("/api/Branding/images/1");

        byNumber.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ShouldStoreEnumsAsNumbersWithASeededLookupTable()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var column = context.Model.FindEntityType(typeof(PortalModule))!.FindProperty(nameof(PortalModule.Type))!;
        column.GetColumnType().ShouldBe("integer");
        column.GetValueConverter().ShouldBeNull();

        var lookup = await context.Set<EnumLookup<ModuleType>>().OrderBy(l => l.Id).ToListAsync();
        lookup.Select(l => (l.Id, l.Name)).ShouldBe(Enum.GetValues<ModuleType>().Select(v => (v, v.ToString())));
    }

    [Test]
    public async Task ShouldRefuseANumberTheEnumDoesNotDefine()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var module = await context.Modules.FirstAsync();

        // The lookup table's foreign key stops a value no member has, even from outside the application.
        await Should.ThrowAsync<PostgresException>(() =>
            context.Database.ExecuteSqlAsync($"UPDATE \"Modules\" SET \"Type\" = {99} WHERE \"Id\" = {module.Id}"));
    }

    [Test]
    public async Task ShouldMakeThePublicIdAnAlternateKey()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        foreach (var type in new[] { typeof(School), typeof(Country), typeof(PortalModule), typeof(SchoolModule), typeof(BrandingImage), typeof(SchoolRequest) })
        {
            var entity = context.Model.FindEntityType(type)!;
            entity.FindPrimaryKey()!.Properties.Single().ClrType.ShouldBe(typeof(long), type.Name);
            entity.GetKeys().ShouldContain(k => !k.IsPrimaryKey() && k.Properties.Single().Name == "PublicId", type.Name);
        }
    }

    private static async Task<HttpSession> SignInAsMasterAsync()
    {
        var master = TestHttp.CreateDevelopmentSession();
        using var response = await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return master;
    }
}

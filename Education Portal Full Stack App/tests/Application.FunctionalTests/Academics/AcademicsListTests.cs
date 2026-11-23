using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.Academics;

/// <summary>
/// The lists behind the Exams and Timetable screens: the assessments of a course and the lessons of the timetable. Each needs its
/// module on for the school; another school's course is not found, and another school's lessons never show (SEC-AUTHZ-03).
/// </summary>
public class AcademicsListTests : TestBase
{
    private const string Domain = "academics.localhost";
    private const string OtherDomain = "other-academics.localhost";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private sealed record SchoolSetUp(EntityIds School, Course Course, HttpSession Session);

    private sealed record AssessmentRead(Guid Id, string Name, decimal Weight, decimal MaximumScore);

    private sealed record SlotRead(Guid Id, Guid CourseId, string CourseName, Weekday Day, string Start, string End, Guid? CampusId, string? Room);

    [Test]
    public async Task ShouldListTheAssessmentsOfACourseByName()
    {
        var setUp = await SetUpAsync("Academics School", Domain, "admin@academics.local", [ModuleCode.Exams]);
        await TestApp.AddAsync(Assessment.Create(setUp.School.Id, setUp.Course.Id, "Final", 60m, 100m));
        await TestApp.AddAsync(Assessment.Create(setUp.School.Id, setUp.Course.Id, "Midterm", 40m, 50m));

        using var response = await setUp.Session.GetAsync($"/api/Assessments?courseId={setUp.Course.PublicId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var list = (await response.Content.ReadFromJsonAsync<List<AssessmentRead>>(Json))!;
        list.Select(a => (a.Name, a.Weight, a.MaximumScore)).ShouldBe([("Final", 60m, 100m), ("Midterm", 40m, 50m)]);
    }

    [Test]
    public async Task ShouldNotFindTheCourseOfAnotherSchool()
    {
        var first = await SetUpAsync("First School", Domain, "admin@first-academics.local", [ModuleCode.Exams]);
        var second = await SetUpAsync("Second School", OtherDomain, "admin@second-academics.local", [ModuleCode.Exams]);

        using var response = await second.Session.GetAsync($"/api/Assessments?courseId={first.Course.PublicId}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ShouldRefuseTheAssessmentsWhenTheExamsModuleIsOff()
    {
        var setUp = await SetUpAsync("No Exams School", Domain, "admin@noexams.local", [ModuleCode.Timetable]);

        using var response = await setUp.Session.GetAsync($"/api/Assessments?courseId={setUp.Course.PublicId}");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldRefuseAnAssessmentListWithoutACourse()
    {
        var setUp = await SetUpAsync("Missing Course School", Domain, "admin@missingcourse.local", [ModuleCode.Exams]);

        using var response = await setUp.Session.GetAsync($"/api/Assessments?courseId={Guid.Empty}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldListTheLessonsOfTheTimetableByDayAndTime()
    {
        var setUp = await SetUpAsync("Timetable School", Domain, "admin@timetable.local", [ModuleCode.Timetable]);
        await TestApp.AddAsync(TimetableSlot.Create(setUp.School.Id, setUp.Course.Id, await TestApp.MainCampusIdAsync(setUp.School.Id), Weekday.Tuesday, new TimeOnly(9, 0), new TimeOnly(10, 0), "Room 2"));
        await TestApp.AddAsync(TimetableSlot.Create(setUp.School.Id, setUp.Course.Id, await TestApp.MainCampusIdAsync(setUp.School.Id), Weekday.Monday, new TimeOnly(11, 0), new TimeOnly(12, 0), null));

        using var response = await setUp.Session.GetAsync("/api/TimetableSlots");
        using var forCourse = await setUp.Session.GetAsync($"/api/TimetableSlots?courseId={setUp.Course.PublicId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var slots = (await response.Content.ReadFromJsonAsync<List<SlotRead>>(Json))!;
        slots.Select(s => (s.Day, s.CourseName, s.Room)).ShouldBe([(Weekday.Monday, "Math", null), (Weekday.Tuesday, "Math", "Room 2")]);
        (await forCourse.Content.ReadFromJsonAsync<List<SlotRead>>(Json))!.Count.ShouldBe(2);
    }

    [Test]
    public async Task ShouldNotListTheLessonsOfAnotherSchool()
    {
        var first = await SetUpAsync("First Timetable School", Domain, "admin@first-timetable.local", [ModuleCode.Timetable]);
        var second = await SetUpAsync("Second Timetable School", OtherDomain, "admin@second-timetable.local", [ModuleCode.Timetable]);
        await TestApp.AddAsync(TimetableSlot.Create(first.School.Id, first.Course.Id, await TestApp.MainCampusIdAsync(first.School.Id), Weekday.Monday, new TimeOnly(9, 0), new TimeOnly(10, 0), null));

        using var own = await second.Session.GetAsync("/api/TimetableSlots");
        using var byOtherCourse = await second.Session.GetAsync($"/api/TimetableSlots?courseId={first.Course.PublicId}");

        (await own.Content.ReadFromJsonAsync<List<SlotRead>>(Json))!.ShouldBeEmpty();
        byOtherCourse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ShouldRefuseTheTimetableWhenItsModuleIsOff()
    {
        var setUp = await SetUpAsync("No Timetable School", Domain, "admin@notimetable.local", [ModuleCode.Exams]);

        using var response = await setUp.Session.GetAsync("/api/TimetableSlots");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    /// <summary>A school with the given paid modules, an administrator signed in, and one course ("Math") with its teacher.</summary>
    private static async Task<SchoolSetUp> SetUpAsync(string name, string domain, string email, ModuleCode[] modules)
    {
        var school = await TestApp.AddSchoolAsync(name, domain);
        await TestApp.SubscribeAsync(school.Id, modules);

        var teacher = UserDetails.Create(school.Id, "Sara", null, "Ali", UserType.Teacher, null);
        await TestApp.AddAsync(teacher);
        var campusId = await TestApp.MainCampusIdAsync(school.Id);
        var offering = ClassOffering.Create(school.Id, campusId, ClassLevel.Class1, "2026-27", 1000m, null, "PKR", 30, 30, AdmissionStatus.Open, true);
        await TestApp.AddAsync(offering);
        var course = Course.Create(school.Id, offering.Id, "Math", teacher.Id, campusId);
        await TestApp.AddAsync(course);

        await TestApp.RunAsUserAsync(school.Id, email, TestApp.AdministratorPassword, [Roles.Administrator]);
        var session = TestHttp.CreateDevelopmentSession(domain);
        using var response = await session.SignInAsync(email, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        return new SchoolSetUp(school, course, session);
    }
}

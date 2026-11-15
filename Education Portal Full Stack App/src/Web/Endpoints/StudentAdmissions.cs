using EducationPortal.Application.StudentAdmissions.Commands.CreateStudentAdmission;
using EducationPortal.Application.StudentAdmissions.Dtos;
using EducationPortal.Application.StudentAdmissions.Queries.GetStudentAdmission;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Endpoints;

/// <summary>
/// Student admissions of the current school: the admission form of each student (Students module). The school is the one
/// the request's host resolves to, so a school never reaches another school's students (AGENTS.md section 2).
/// </summary>
public class StudentAdmissions : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(CreateStudentAdmission)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapGet(GetStudentAdmission, "{id:guid}")
            .RequireAuthorization();
    }

    [EndpointSummary("Admit a student")]
    [EndpointDescription("Adds a student and the basic fields of the admission form (date of birth, gender, father's name, nationality, address, city, class applied for, previous school, guardian phone, admission date). The student must be 3 to 30 years old on the admission date. Needs the Students module.")]
    public static async Task<Created<Guid>> CreateStudentAdmission(ISender sender, [FromBody] CreateStudentAdmissionCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/api/StudentAdmissions/{id}", id);
    }

    [EndpointSummary("Get a student's admission form")]
    [EndpointDescription("Returns one student's admission form, named by the student's id. Needs the Students module.")]
    public static async Task<Ok<StudentAdmissionDto>> GetStudentAdmission(ISender sender, Guid id)
    {
        return TypedResults.Ok(await sender.Send(new GetStudentAdmissionQuery(id)));
    }
}

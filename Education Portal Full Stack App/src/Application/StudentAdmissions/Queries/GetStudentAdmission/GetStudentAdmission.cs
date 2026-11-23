using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.StudentAdmissions.Dtos;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.Application.StudentAdmissions.Queries.GetStudentAdmission;

/// <summary>
/// Reads one student's admission form, named by the student's public id. The school is the one the request's host resolves
/// to, so another school's student is 404 (SEC-AUTHZ-04). Needs the Students module.
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.Students)]
public record GetStudentAdmissionQuery(Guid Id);

public class GetStudentAdmissionQueryHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public GetStudentAdmissionQueryHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<StudentAdmissionDto> Handle(GetStudentAdmissionQuery request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId;

        return await _context.StudentProfiles
            .AsNoTracking()
            .Where(s => s.SchoolId == schoolId && s.UserDetails.PublicId == request.Id)
            .Select(s => new StudentAdmissionDto(
                s.UserDetails.PublicId,
                s.UserDetails.FirstName,
                s.UserDetails.MiddleName,
                s.UserDetails.LastName,
                s.UserDetails.Phone,
                s.DateOfBirth,
                s.Gender,
                s.FatherName,
                s.Nationality,
                s.Address,
                s.City,
                s.ClassApplied,
                s.PreviousSchool,
                s.GuardianPhone,
                s.AdmissionDate))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(request.Id.ToString(), nameof(StudentProfile));
    }
}

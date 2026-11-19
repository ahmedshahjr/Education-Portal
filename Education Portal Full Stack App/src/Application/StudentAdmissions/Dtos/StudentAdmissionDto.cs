using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.StudentAdmissions.Dtos;

/// <summary>A student's admission form with the student's name and phone. Id is the student's person public id.</summary>
public sealed record StudentAdmissionDto(
    Guid Id,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? Phone,
    DateOnly DateOfBirth,
    Gender Gender,
    string FatherName,
    string Nationality,
    string Address,
    string City,
    string ClassApplied,
    string? PreviousSchool,
    string GuardianPhone,
    DateOnly AdmissionDate);

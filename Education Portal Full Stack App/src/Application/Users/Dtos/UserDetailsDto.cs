using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Users.Dtos;

/// <summary>A person of the school: a principal, a teacher or other staff, with the person's name and contact.</summary>
public sealed record UserDetailsDto(Guid Id, string FirstName, string? MiddleName, string LastName, UserType UserType, string? Phone);

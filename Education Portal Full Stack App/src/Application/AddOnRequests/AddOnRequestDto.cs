namespace EducationPortal.Application.AddOnRequests;

/// <summary>A school's request for an add-on module, as the school and the master tenant see it.</summary>
public sealed record AddOnRequestDto(
    Guid Id,
    Guid SchoolId,
    string SchoolName,
    string ModuleCode,
    string ModuleName,
    AddOnRequestStatus Status,
    DateTimeOffset Created,
    DateTimeOffset? DecidedAt);

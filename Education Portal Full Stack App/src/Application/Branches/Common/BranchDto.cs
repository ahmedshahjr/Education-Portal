using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Branches.Common;

/// <summary>
/// A campus of a school (the class keeps its name). <see cref="Code"/> is the four-digit code; <see cref="DisplayCode"/> is
/// the code as the school shows it, with the school's prefix when the prefix is on. <see cref="Kind"/> is
/// <see cref="BranchKind.Main"/> for the registration campus.
/// </summary>
public sealed record BranchDto(Guid Id, string Name, string Code, string DisplayCode, BranchKind Kind);

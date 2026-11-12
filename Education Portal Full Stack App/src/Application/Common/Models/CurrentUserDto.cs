namespace EducationPortal.Application.Common.Models;

/// <summary>The signed-in user's own account, as shown to that user.</summary>
public sealed record CurrentUserDto(string Id, string Email, IReadOnlyList<string> Roles);

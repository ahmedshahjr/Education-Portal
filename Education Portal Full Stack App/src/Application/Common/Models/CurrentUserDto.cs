namespace EducationPortal.Application.Common.Models;

/// <summary>
/// The signed-in user's own account, as shown to that user. <paramref name="MustChangePassword"/> is true while the
/// account still has its one-time password; the app then shows only the change-password page.
/// </summary>
public sealed record CurrentUserDto(string Id, string Email, IReadOnlyList<string> Roles, bool MustChangePassword);

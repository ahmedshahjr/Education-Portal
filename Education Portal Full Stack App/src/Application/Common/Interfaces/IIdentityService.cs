using EducationPortal.Application.Common.Models;

namespace EducationPortal.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(int schoolId, string userName, string password);

    Task<Result> DeleteUserAsync(string userId);

    /// <summary>True when any account, in any school, already uses the email (emails are unique platform-wide).</summary>
    Task<bool> IsEmailTakenAsync(string email);

    /// <summary>
    /// Creates a school's administrator with a one-time password, checked against the password policy. The account must
    /// replace the password before it can do anything else.
    /// </summary>
    Task<(Result Result, string UserId)> CreateSchoolAdministratorAsync(int schoolId, string email, string oneTimePassword);

    /// <summary>
    /// Replaces the user's password after checking the current one, ends the one-time-password block, ends their other
    /// sessions (new security stamp) and renews the current one (SEC-AUTHN-07).
    /// </summary>
    Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

    /// <summary>
    /// Checks the password and, when it matches, signs the user in with the session cookie. Failed
    /// attempts count towards lockout (SEC-AUTHN-05). An account of another school than
    /// <paramref name="schoolId"/> (the school of the request's domain) is refused like a wrong password,
    /// without counting towards its lockout.
    /// </summary>
    Task<PasswordSignInResult> PasswordSignInAsync(int schoolId, string email, string password);

    /// <summary>
    /// Signs the user out and ends every session they have, not only the current browser's (SEC-AUTHN-07).
    /// </summary>
    Task SignOutAsync(string userId);

    Task<CurrentUserDto?> GetCurrentUserAsync(string userId);

    /// <summary>Saves the user's language on their account, so it follows them to other devices.</summary>
    Task<Result> SetPreferredLanguageAsync(string userId, string language);
}

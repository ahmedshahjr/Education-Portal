using EducationPortal.Application.Common.Models;

namespace EducationPortal.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(int schoolId, string userName, string password);

    Task<Result> DeleteUserAsync(string userId);

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
}

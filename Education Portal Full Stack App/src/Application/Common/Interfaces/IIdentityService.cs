using EducationPortal.Application.Common.Models;

namespace EducationPortal.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(string userId);

    /// <summary>
    /// Checks the password and, when it matches, signs the user in with the session cookie. Failed
    /// attempts count towards lockout (SEC-AUTHN-05).
    /// </summary>
    Task<PasswordSignInResult> PasswordSignInAsync(string email, string password);

    /// <summary>
    /// Signs the user out and ends every session they have, not only the current browser's (SEC-AUTHN-07).
    /// </summary>
    Task SignOutAsync(string userId);

    Task<CurrentUserDto?> GetCurrentUserAsync(string userId);
}

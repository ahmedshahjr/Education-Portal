using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace EducationPortal.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    // Computed once, on first use, and only used to spend the same time on an unknown email as on a real one.
    private static string? _dummyPasswordHash;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(int schoolId, string userName, string password)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName,
            SchoolId = schoolId,
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> IsEmailTakenAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email) is not null;
    }

    public async Task<(Result Result, string UserId)> CreateSchoolAdministratorAsync(int schoolId, string email, string oneTimePassword)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            SchoolId = schoolId,
            MustChangePassword = true,
        };

        var created = await _userManager.CreateAsync(user, oneTimePassword);

        if (!created.Succeeded)
        {
            return (created.ToApplicationResult(), string.Empty);
        }

        var inRole = await _userManager.AddToRoleAsync(user, Roles.Administrator);

        return (inRole.ToApplicationResult(), user.Id);
    }

    public async Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Result.Failure(["The user does not exist."]);
        }

        // Checks the current password and the policy, saves the new hash and rotates the security stamp, which ends
        // every other session of the user.
        var changed = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (!changed.Succeeded)
        {
            return changed.ToApplicationResult();
        }

        if (user.MustChangePassword)
        {
            user.MustChangePassword = false;
            var updated = await _userManager.UpdateAsync(user);

            if (!updated.Succeeded)
            {
                return updated.ToApplicationResult();
            }
        }

        // The stamp changed, so this browser's cookie is renewed too; otherwise the next request would sign it out.
        await _signInManager.RefreshSignInAsync(user);

        return Result.Success();
    }

    public async Task<Result> SetPreferredLanguageAsync(string userId, string language)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Result.Failure(["The user does not exist."]);
        }

        if (string.Equals(user.PreferredLanguage, language, StringComparison.Ordinal))
        {
            return Result.Success();
        }

        user.PreferredLanguage = language;

        return (await _userManager.UpdateAsync(user)).ToApplicationResult();
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }

    public async Task<PasswordSignInResult> PasswordSignInAsync(int schoolId, string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null || user.SchoolId != schoolId)
        {
            // Hash the password anyway, so the response time does not show whether the email has an account. An account
            // of another school is treated the same way, and its failed attempts do not count towards its lockout, so
            // one school's domain cannot be used to lock out another school's users.
            _userManager.PasswordHasher.VerifyHashedPassword(new ApplicationUser(), DummyPasswordHash(), password);

            return user is null
                ? new PasswordSignInResult(PasswordSignInStatus.Failed, null)
                : new PasswordSignInResult(PasswordSignInStatus.WrongSchool, user.Id);
        }

        // lockoutOnFailure counts failed attempts and locks the account after three of them (SEC-AUTHN-05).
        var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: true);

        var status = result switch
        {
            { Succeeded: true } => PasswordSignInStatus.Succeeded,
            { IsLockedOut: true } => PasswordSignInStatus.LockedOut,
            { RequiresTwoFactor: true } => PasswordSignInStatus.RequiresTwoFactor,
            { IsNotAllowed: true } => PasswordSignInStatus.NotAllowed,
            _ => PasswordSignInStatus.Failed,
        };

        return new PasswordSignInResult(status, user.Id);
    }

    public async Task SignOutAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is not null)
        {
            // A new security stamp makes every session cookie of this user invalid on its next request (SEC-AUTHN-07).
            await _userManager.UpdateSecurityStampAsync(user);
        }

        await _signInManager.SignOutAsync();
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new CurrentUserDto(user.Id, user.Email ?? string.Empty, roles.ToList(), user.MustChangePassword);
    }

    private string DummyPasswordHash()
    {
        return _dummyPasswordHash ??= _userManager.PasswordHasher.HashPassword(
            new ApplicationUser(),
            "a password that no account uses");
    }
}

using System.IO.Compression;
using System.Text;
using EducationPortal.Application.Common.Security;
using Microsoft.AspNetCore.Identity;

namespace EducationPortal.Infrastructure.Identity;

/// <summary>
/// The part of the password policy that Identity does not cover (SEC-AUTHN-03): the maximum length, a list of
/// common passwords, and words taken from the account itself. Identity's own validator covers the minimum length.
/// </summary>
public sealed class PasswordPolicyValidator : IPasswordValidator<ApplicationUser>
{
    private const string CommonPasswordsResource = "EducationPortal.Infrastructure.Identity.CommonPasswords.txt.gz";
    private const string ProductWord = "educationportal";
    private const int MinimumPersonalWordLength = 4;

    private static readonly Lazy<HashSet<string>> CommonPasswords = new(LoadCommonPasswords);

    public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string? password)
    {
        // An empty password is reported by Identity's own validator.
        if (string.IsNullOrEmpty(password))
        {
            return Task.FromResult(IdentityResult.Success);
        }

        if (password.Length > PasswordRules.MaximumLength)
        {
            return Task.FromResult(Fail("PasswordTooLong", $"The password can be at most {PasswordRules.MaximumLength} characters."));
        }

        var normalised = password.ToLowerInvariant();

        if (CommonPasswords.Value.Contains(normalised))
        {
            return Task.FromResult(Fail("CommonPassword", "This password is too common. Choose a longer, less predictable one."));
        }

        if (ContainsPersonalWord(normalised, user))
        {
            return Task.FromResult(Fail("PersonalWord", "The password must not contain the account name or the product name."));
        }

        return Task.FromResult(IdentityResult.Success);
    }

    private static bool ContainsPersonalWord(string normalisedPassword, ApplicationUser user)
    {
        if (normalisedPassword.Contains(ProductWord, StringComparison.Ordinal))
        {
            return true;
        }

        var localPart = user.Email?.Split('@')[0].ToLowerInvariant();
        var userName = user.UserName?.ToLowerInvariant();

        foreach (var word in new[] { localPart, userName })
        {
            if (word is { Length: >= MinimumPersonalWordLength }
                && normalisedPassword.Contains(word, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static HashSet<string> LoadCommonPasswords()
    {
        using var resource = typeof(PasswordPolicyValidator).Assembly.GetManifestResourceStream(CommonPasswordsResource)
            ?? throw new InvalidOperationException($"The embedded resource '{CommonPasswordsResource}' is missing.");
        using var gzip = new GZipStream(resource, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, Encoding.UTF8);

        // The list is already lower-case, so the lookup uses the lower-cased password.
        var passwords = new HashSet<string>(StringComparer.Ordinal);

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (line.Length > 0)
            {
                passwords.Add(line);
            }
        }

        return passwords;
    }

    private static IdentityResult Fail(string code, string description)
    {
        return IdentityResult.Failed(new IdentityError { Code = code, Description = description });
    }
}

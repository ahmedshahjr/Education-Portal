using System.Globalization;
using System.Text.RegularExpressions;

namespace EducationPortal.Application.Common.Secrets;

/// <summary>
/// A logical secret key (ADR-006). Application code names secrets only through this type, and each secret provider
/// translates the key to its own naming rules. There are two kinds:
/// <list type="bullet">
/// <item><c>platform/{name}</c>: platform secrets, for example <c>platform/db-connection</c>.</item>
/// <item><c>tenants/{schoolId}/{name}</c>: secrets of one school, for example <c>tenants/5/sms-api-key</c>.</item>
/// </list>
/// A name is 1 to 63 lowercase letters, digits and dashes and starts with a letter. The rule keeps every provider's
/// translation unambiguous and rules out path tricks such as <c>../</c>.
/// </summary>
public sealed partial record SecretKey
{
    public const string PlatformPrefix = "platform";
    public const string TenantPrefix = "tenants";
    public const int MaxNameLength = 63;

    private SecretKey(int? tenantId, string name)
    {
        TenantId = tenantId;
        Name = name;
        Path = tenantId is null
            ? $"{PlatformPrefix}/{name}"
            : $"{TenantPrefix}/{tenantId.Value.ToString(CultureInfo.InvariantCulture)}/{name}";
    }

    /// <summary>The school a tenant secret belongs to, or <c>null</c> for a platform secret.</summary>
    public int? TenantId { get; }

    /// <summary>The secret's name without its prefix, for example <c>sms-api-key</c>.</summary>
    public string Name { get; }

    /// <summary>The logical key, for example <c>tenants/5/sms-api-key</c>.</summary>
    public string Path { get; }

    public bool IsPlatform => TenantId is null;

    public static SecretKey Platform(string name) => new(null, ValidName(name));

    /// <summary>
    /// A tenant secret. Application code does not call this: it uses <c>ITenantSecretProvider</c>, which takes the
    /// school from the request's host (SEC-AUTHZ-04).
    /// </summary>
    public static SecretKey ForTenant(int tenantId, string name)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tenantId), tenantId, "A tenant id must be positive.");
        }

        return new SecretKey(tenantId, ValidName(name));
    }

    /// <summary>Reads a logical key such as <c>platform/db-connection</c> or <c>tenants/5/sms-api-key</c>.</summary>
    public static SecretKey Parse(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var parts = path.Split('/');

        if (parts is [PlatformPrefix, var platformName])
        {
            return Platform(platformName);
        }

        if (parts is [TenantPrefix, var tenant, var tenantName]
            && TenantIdPattern().IsMatch(tenant)
            && int.TryParse(tenant, NumberStyles.None, CultureInfo.InvariantCulture, out var tenantId))
        {
            return ForTenant(tenantId, tenantName);
        }

        throw new ArgumentException(
            "A secret key is 'platform/{name}' or 'tenants/{tenantId}/{name}'.", nameof(path));
    }

    public static bool IsValidName(string? name) => name is not null && NamePattern().IsMatch(name);

    public override string ToString() => Path;

    private static string ValidName(string name)
    {
        if (!IsValidName(name))
        {
            throw new ArgumentException(
                $"A secret name is 1 to {MaxNameLength} lowercase letters, digits and dashes, starting with a letter.",
                nameof(name));
        }

        return name;
    }

    [GeneratedRegex("^[a-z][a-z0-9-]{0,62}$", RegexOptions.CultureInvariant)]
    private static partial Regex NamePattern();

    [GeneratedRegex("^[1-9][0-9]{0,9}$", RegexOptions.CultureInvariant)]
    private static partial Regex TenantIdPattern();
}

using Microsoft.Extensions.Options;

namespace EducationPortal.Infrastructure.Secrets;

/// <summary>
/// Checks the <c>Secrets</c> section at startup, so a missing setting stops the app with a clear message instead of
/// failing on the first secret read, which fails closed.
/// </summary>
public sealed class SecretsOptionsValidator : IValidateOptions<SecretsOptions>
{
    private const string CredentialHint =
        "Set it with an environment variable or dotnet user-secrets, never in appsettings.json.";

    public ValidateOptionsResult Validate(string? name, SecretsOptions options)
    {
        var errors = GetErrors(options);
        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }

    public static IReadOnlyList<string> GetErrors(SecretsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var errors = new List<string>();

        if (options.Kind is not { } kind)
        {
            errors.Add($"Secrets:Provider '{options.Provider}' is not supported. Use HashiCorp, AzureKeyVault, Aws or Gcp, or leave it empty.");
            return errors;
        }

        if (kind == SecretProviderKind.None)
        {
            return errors;
        }

        if (options.CacheTtlMinutes is < 1 or > 1440)
        {
            errors.Add("Secrets:CacheTtlMinutes must be between 1 and 1440.");
        }

        switch (kind)
        {
            case SecretProviderKind.HashiCorp:
                ValidateHashiCorp(options.HashiCorp, errors);
                break;
            case SecretProviderKind.AzureKeyVault:
                if (!Uri.TryCreate(options.AzureKeyVault.VaultUri, UriKind.Absolute, out var vaultUri) || vaultUri.Scheme != Uri.UriSchemeHttps)
                {
                    errors.Add("Secrets:Provider is 'AzureKeyVault' but Secrets:AzureKeyVault:VaultUri is missing or not an https URI.");
                }

                break;
            case SecretProviderKind.Aws:
                if (string.IsNullOrWhiteSpace(options.Aws.Region))
                {
                    errors.Add("Secrets:Provider is 'Aws' but Secrets:Aws:Region is missing.");
                }

                break;
            case SecretProviderKind.Gcp:
                if (string.IsNullOrWhiteSpace(options.Gcp.ProjectId))
                {
                    errors.Add("Secrets:Provider is 'Gcp' but Secrets:Gcp:ProjectId is missing.");
                }

                break;
        }

        return errors;
    }

    private static void ValidateHashiCorp(HashiCorpVaultOptions options, List<string> errors)
    {
        if (!Uri.TryCreate(options.Address, UriKind.Absolute, out var address)
            || (address.Scheme != Uri.UriSchemeHttp && address.Scheme != Uri.UriSchemeHttps))
        {
            errors.Add("Secrets:Provider is 'HashiCorp' but Secrets:HashiCorp:Address is missing or not an http(s) URI.");
        }

        if (string.IsNullOrWhiteSpace(options.MountPoint))
        {
            errors.Add("Secrets:HashiCorp:MountPoint is missing.");
        }

        switch (options.AuthMethod)
        {
            case HashiCorpAuthMethod.Token when string.IsNullOrWhiteSpace(options.Token):
                errors.Add($"Secrets:HashiCorp:AuthMethod is 'Token' but Secrets:HashiCorp:Token is missing. {CredentialHint}");
                break;
            case HashiCorpAuthMethod.AppRole when string.IsNullOrWhiteSpace(options.RoleId) || string.IsNullOrWhiteSpace(options.SecretId):
                errors.Add($"Secrets:HashiCorp:AuthMethod is 'AppRole' but Secrets:HashiCorp:RoleId or Secrets:HashiCorp:SecretId is missing. {CredentialHint}");
                break;
        }
    }
}

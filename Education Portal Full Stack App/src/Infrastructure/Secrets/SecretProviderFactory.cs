using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Infrastructure.Secrets;

/// <summary>Creates the store that <c>Secrets:Provider</c> selects. Call it only with validated options.</summary>
public static class SecretProviderFactory
{
    public static ISecretProvider Create(SecretsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.Kind switch
        {
            SecretProviderKind.HashiCorp => new HashiCorpVaultSecretProvider(
                HashiCorpVaultSecretProvider.CreateClient(options.HashiCorp), options.HashiCorp.MountPoint),
            SecretProviderKind.AzureKeyVault => new AzureKeyVaultSecretProvider(
                AzureKeyVaultSecretProvider.CreateClient(options.AzureKeyVault)),
            SecretProviderKind.Aws => new AwsSecretsManagerSecretProvider(
                AwsSecretsManagerSecretProvider.CreateClient(options.Aws)),
            SecretProviderKind.Gcp => new GcpSecretManagerSecretProvider(
                GcpSecretManagerSecretProvider.CreateClient(), options.Gcp.ProjectId!),
            _ => new NotConfiguredSecretProvider(),
        };
    }
}

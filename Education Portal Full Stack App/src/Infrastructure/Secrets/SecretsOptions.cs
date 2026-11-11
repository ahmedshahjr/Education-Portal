namespace EducationPortal.Infrastructure.Secrets;

/// <summary>The secret stores that <c>Secrets:Provider</c> can select (ADR-006).</summary>
public enum SecretProviderKind
{
    /// <summary><c>Secrets:Provider</c> is empty: no secret store, the app runs on normal configuration.</summary>
    None,
    HashiCorp,
    AzureKeyVault,
    Aws,
    Gcp,
}

/// <summary>
/// The <c>Secrets</c> configuration section: one secret store per environment, shared by every tenant (ADR-006).
/// Credentials (the Vault token, the AppRole role id and secret id) come from environment variables or user-secrets,
/// never from <c>appsettings*.json</c> (SEC-SECRET-01). The cloud stores use their platform identity and need none.
/// </summary>
public sealed class SecretsOptions
{
    public const string SectionName = "Secrets";

    public const int DefaultCacheTtlMinutes = 10;

    /// <summary><c>HashiCorp</c>, <c>AzureKeyVault</c>, <c>Aws</c> or <c>Gcp</c>; empty for none.</summary>
    public string? Provider { get; set; }

    /// <summary>How long a secret value is cached in memory, in minutes (1 to 1440).</summary>
    public int CacheTtlMinutes { get; set; } = DefaultCacheTtlMinutes;

    public HashiCorpVaultOptions HashiCorp { get; set; } = new();

    public AzureKeyVaultOptions AzureKeyVault { get; set; } = new();

    public AwsSecretsManagerOptions Aws { get; set; } = new();

    public GcpSecretManagerOptions Gcp { get; set; } = new();

    /// <summary>The selected store, or <c>null</c> when <see cref="Provider"/> is not a known name.</summary>
    public SecretProviderKind? Kind =>
        string.IsNullOrWhiteSpace(Provider) ? SecretProviderKind.None
        : Enum.TryParse<SecretProviderKind>(Provider, ignoreCase: true, out var kind) && kind != SecretProviderKind.None ? kind
        : null;
}

public enum HashiCorpAuthMethod
{
    /// <summary>A Vault token, for local development (<c>Secrets__HashiCorp__Token</c>).</summary>
    Token,

    /// <summary>AppRole (<c>Secrets__HashiCorp__RoleId</c> and <c>Secrets__HashiCorp__SecretId</c>).</summary>
    AppRole,
}

public sealed class HashiCorpVaultOptions
{
    /// <summary>For example <c>http://localhost:8200</c>. Aspire sets it for the local Vault container.</summary>
    public string? Address { get; set; }

    /// <summary>The KV version 2 secrets engine's mount point.</summary>
    public string MountPoint { get; set; } = "secret";

    public HashiCorpAuthMethod AuthMethod { get; set; } = HashiCorpAuthMethod.Token;

    /// <summary>Environment variable or user-secret only.</summary>
    public string? Token { get; set; }

    /// <summary>Environment variable or user-secret only.</summary>
    public string? RoleId { get; set; }

    /// <summary>Environment variable or user-secret only.</summary>
    public string? SecretId { get; set; }

    /// <summary>The AppRole auth method's mount point.</summary>
    public string AppRoleMountPoint { get; set; } = "approle";
}

public sealed class AzureKeyVaultOptions
{
    /// <summary>For example <c>https://my-vault.vault.azure.net/</c>. Access uses DefaultAzureCredential.</summary>
    public string? VaultUri { get; set; }
}

public sealed class AwsSecretsManagerOptions
{
    /// <summary>For example <c>eu-west-1</c>. Access uses the default credential chain (IAM role).</summary>
    public string? Region { get; set; }
}

public sealed class GcpSecretManagerOptions
{
    /// <summary>The Google Cloud project id. Access uses application default credentials.</summary>
    public string? ProjectId { get; set; }
}

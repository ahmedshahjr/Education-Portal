using EducationPortal.Application.Common.Secrets;
using EducationPortal.Infrastructure.Secrets;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Infrastructure.IntegrationTests.Secrets;

/// <summary>Each store's translation of logical keys to its own naming rules (ADR-006).</summary>
public class KeyTranslationTests
{
    private static readonly SecretKey Platform = SecretKey.Platform("db-connection");
    private static readonly SecretKey Tenant = SecretKey.ForTenant(5, "sms-api-key");

    [Test]
    public void HashiCorpShouldUseTheLogicalPath()
    {
        HashiCorpVaultSecretProvider.ToSecretPath(Platform).ShouldBe("platform/db-connection");
        HashiCorpVaultSecretProvider.ToSecretPath(Tenant).ShouldBe("tenants/5/sms-api-key");
    }

    [Test]
    public void AzureKeyVaultShouldUseLettersDigitsAndDashesOnly()
    {
        AzureKeyVaultSecretProvider.ToSecretName(Platform).ShouldBe("platform-db-connection");
        AzureKeyVaultSecretProvider.ToSecretName(Tenant).ShouldBe("tenant-5-sms-api-key");
    }

    [Test]
    public void AwsShouldUseTheLogicalPath()
    {
        AwsSecretsManagerSecretProvider.ToSecretName(Platform).ShouldBe("platform/db-connection");
        AwsSecretsManagerSecretProvider.ToSecretName(Tenant).ShouldBe("tenants/5/sms-api-key");
    }

    [Test]
    public void GcpShouldUseLettersDigitsAndDashesOnly()
    {
        GcpSecretManagerSecretProvider.ToSecretId(Platform).ShouldBe("platform-db-connection");
        GcpSecretManagerSecretProvider.ToSecretId(Tenant).ShouldBe("tenant-5-sms-api-key");
    }

    [Test]
    public void DashedNamesShouldMatchAzureAndGcpRules()
    {
        var longest = SecretKey.ForTenant(long.MaxValue, "a" + new string('z', SecretKey.MaxNameLength - 1));
        var name = AzureKeyVaultSecretProvider.ToSecretName(longest);

        // Azure Key Vault: 1-127 characters, letters, digits and dashes. Google Secret Manager: up to 255 characters,
        // letters, digits, dashes and underscores.
        name.Length.ShouldBeLessThanOrEqualTo(127);
        name.ShouldMatch("^[a-zA-Z0-9-]+$");
    }

    [Test]
    public void DashedNamesShouldNeverCollide()
    {
        // The name rule (starts with a letter) keeps the tenant id boundary unambiguous.
        var keys = new[]
        {
            SecretKey.ForTenant(1, "a2-x"),
            SecretKey.ForTenant(12, "x"),
            SecretKey.ForTenant(1, "x"),
            SecretKey.Platform("tenant-1-x"),
            SecretKey.Platform("x"),
        };

        keys.Select(AzureKeyVaultSecretProvider.ToSecretName).Distinct().Count().ShouldBe(keys.Length);
    }
}

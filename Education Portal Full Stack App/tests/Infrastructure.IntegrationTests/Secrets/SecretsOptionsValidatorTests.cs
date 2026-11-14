using EducationPortal.Infrastructure.Secrets;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Infrastructure.IntegrationTests.Secrets;

/// <summary>Startup validation of the Secrets section: each provider's settings and clear messages.</summary>
public class SecretsOptionsValidatorTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("  ")]
    public void ShouldAcceptNoProvider(string? provider)
    {
        SecretsOptionsValidator.GetErrors(new SecretsOptions { Provider = provider }).ShouldBeEmpty();
    }

    [Test]
    public void ShouldRejectAnUnknownProvider()
    {
        SecretsOptionsValidator.GetErrors(new SecretsOptions { Provider = "Dropbox" })
            .ShouldHaveSingleItem().ShouldContain("'Dropbox' is not supported");
        SecretsOptionsValidator.GetErrors(new SecretsOptions { Provider = "None" }).ShouldNotBeEmpty();
    }

    [Test]
    public void ShouldAcceptHashiCorpWithAToken()
    {
        SecretsOptionsValidator.GetErrors(HashiCorp(token: "t")).ShouldBeEmpty();
    }

    [Test]
    public void ShouldRejectHashiCorpWithoutAToken()
    {
        SecretsOptionsValidator.GetErrors(HashiCorp(token: null))
            .ShouldHaveSingleItem().ShouldContain("Secrets:HashiCorp:Token is missing");
    }

    [Test]
    public void ShouldRejectHashiCorpWithoutAnAddress()
    {
        var options = HashiCorp(token: "t");
        options.HashiCorp.Address = "";

        SecretsOptionsValidator.GetErrors(options).ShouldHaveSingleItem().ShouldContain("Secrets:HashiCorp:Address");
    }

    [Test]
    public void ShouldCheckAppRoleCredentials()
    {
        var options = HashiCorp(token: null);
        options.HashiCorp.AuthMethod = HashiCorpAuthMethod.AppRole;
        options.HashiCorp.RoleId = "role";

        SecretsOptionsValidator.GetErrors(options).ShouldHaveSingleItem().ShouldContain("SecretId");

        options.HashiCorp.SecretId = "secret";
        SecretsOptionsValidator.GetErrors(options).ShouldBeEmpty();
    }

    [TestCase("https://my-vault.vault.azure.net/", true)]
    [TestCase("http://my-vault.vault.azure.net/", false)]
    [TestCase("", false)]
    [TestCase("not a uri", false)]
    public void ShouldCheckTheAzureVaultUri(string uri, bool valid)
    {
        var options = new SecretsOptions { Provider = "AzureKeyVault" };
        options.AzureKeyVault.VaultUri = uri;

        SecretsOptionsValidator.GetErrors(options).Count.ShouldBe(valid ? 0 : 1);
    }

    [Test]
    public void ShouldCheckTheAwsRegionAndGcpProject()
    {
        SecretsOptionsValidator.GetErrors(new SecretsOptions { Provider = "Aws" })
            .ShouldHaveSingleItem().ShouldContain("Secrets:Aws:Region");
        SecretsOptionsValidator.GetErrors(new SecretsOptions { Provider = "gcp" })
            .ShouldHaveSingleItem().ShouldContain("Secrets:Gcp:ProjectId");
    }

    [TestCase(0, false)]
    [TestCase(1, true)]
    [TestCase(1440, true)]
    [TestCase(1441, false)]
    public void ShouldCheckTheCacheTtl(int minutes, bool valid)
    {
        var options = HashiCorp(token: "t");
        options.CacheTtlMinutes = minutes;

        SecretsOptionsValidator.GetErrors(options).Count.ShouldBe(valid ? 0 : 1);
    }

    private static SecretsOptions HashiCorp(string? token) => new()
    {
        Provider = "HashiCorp",
        HashiCorp = { Address = "http://localhost:8200", Token = token },
    };
}

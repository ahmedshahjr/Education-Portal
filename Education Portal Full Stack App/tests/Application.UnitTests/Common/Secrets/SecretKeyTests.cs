using EducationPortal.Application.Common.Secrets;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Common.Secrets;

/// <summary>Logical secret keys and the name rule that keeps them safe to translate (ADR-006).</summary>
public class SecretKeyTests
{
    [Test]
    public void ShouldBuildAPlatformKey()
    {
        var key = SecretKey.Platform("db-connection");

        key.Path.ShouldBe("platform/db-connection");
        key.IsPlatform.ShouldBeTrue();
        key.TenantId.ShouldBeNull();
    }

    [Test]
    public void ShouldBuildATenantKey()
    {
        var key = SecretKey.ForTenant(5, "sms-api-key");

        key.Path.ShouldBe("tenants/5/sms-api-key");
        key.IsPlatform.ShouldBeFalse();
        key.TenantId.ShouldBe(5);
        key.Name.ShouldBe("sms-api-key");
    }

    [TestCase("platform/db-connection")]
    [TestCase("tenants/5/sms-api-key")]
    [TestCase("tenants/2147483647/x")]
    [TestCase("tenants/9223372036854775807/x")] // long.MaxValue: tenant ids are long (ENG-ID-01)
    public void ShouldParseAndRoundTrip(string path)
    {
        SecretKey.Parse(path).Path.ShouldBe(path);
    }

    [Test]
    public void ShouldTreatEqualKeysAsEqual()
    {
        SecretKey.Parse("tenants/5/sms-api-key").ShouldBe(SecretKey.ForTenant(5, "sms-api-key"));
        SecretKey.ForTenant(5, "sms-api-key").ShouldNotBe(SecretKey.ForTenant(6, "sms-api-key"));
    }

    [TestCase("")]
    [TestCase("platform")]
    [TestCase("platform/")]
    [TestCase("platform/a/b")]
    [TestCase("tenants/5")]
    [TestCase("tenants/0/name")]
    [TestCase("tenants/-1/name")]
    [TestCase("tenants/05/name")]
    [TestCase("tenants/abc/name")]
    [TestCase("tenants/9223372036854775808/name")] // one more than long.MaxValue
    [TestCase("tenants/99999999999999999999/name")]
    [TestCase("tenants/5/../6/name")]
    [TestCase("tenants/5/name/extra")]
    [TestCase("other/name")]
    [TestCase("/platform/name")]
    public void ShouldRejectMalformedKeys(string path)
    {
        Should.Throw<ArgumentException>(() => SecretKey.Parse(path));
    }

    [TestCase("a")]
    [TestCase("sms-api-key")]
    [TestCase("key2")]
    public void ShouldAcceptValidNames(string name)
    {
        SecretKey.IsValidName(name).ShouldBeTrue();
    }

    [Test]
    public void ShouldAcceptANameOfTheMaximumLength()
    {
        SecretKey.IsValidName("a" + new string('b', SecretKey.MaxNameLength - 1)).ShouldBeTrue();
        SecretKey.IsValidName("a" + new string('b', SecretKey.MaxNameLength)).ShouldBeFalse();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("1key")]
    [TestCase("-key")]
    [TestCase("Key")]
    [TestCase("sms_api_key")]
    [TestCase("sms.api.key")]
    [TestCase("..")]
    [TestCase("a/b")]
    [TestCase("a b")]
    [TestCase("ключ")]
    public void ShouldRejectInvalidNames(string? name)
    {
        SecretKey.IsValidName(name).ShouldBeFalse();
        Should.Throw<ArgumentException>(() => SecretKey.Platform(name!));
        Should.Throw<ArgumentException>(() => SecretKey.ForTenant(1, name!));
    }

    [TestCase(0)]
    [TestCase(-3)]
    public void ShouldRejectANonPositiveTenantId(long tenantId)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => SecretKey.ForTenant(tenantId, "name"));
    }
}

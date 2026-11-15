using EducationPortal.Application.Campuses.Queries.GetCampusMapSettings;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Secrets;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Campuses;

/// <summary>The map key comes from the secret store; with none set, the picker is told so and takes typed coordinates.</summary>
public class GetCampusMapSettingsQueryHandlerTests
{
    private static async Task<string?> KeyWhenStoredAsync(string? stored)
    {
        var secrets = new Mock<ISecretProvider>();
        secrets
            .Setup(s => s.GetSecretAsync(GetCampusMapSettingsQueryHandler.GoogleMapsApiKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);

        var handler = new GetCampusMapSettingsQueryHandler(secrets.Object);
        var settings = await handler.Handle(new GetCampusMapSettingsQuery(), CancellationToken.None);

        return settings.ApiKey;
    }

    [Test]
    public async Task ShouldReturnTheStoredKeyWithoutSurroundingSpaces()
    {
        (await KeyWhenStoredAsync("  AIza-test-key  ")).ShouldBe("AIza-test-key");
    }

    [Test]
    public async Task ShouldReturnNoKeyWhenNoneIsStored()
    {
        (await KeyWhenStoredAsync(null)).ShouldBeNull();
    }

    [TestCase("")]
    [TestCase("   ")]
    public async Task ShouldTreatABlankKeyAsNoKey(string blank)
    {
        (await KeyWhenStoredAsync(blank)).ShouldBeNull();
    }
}

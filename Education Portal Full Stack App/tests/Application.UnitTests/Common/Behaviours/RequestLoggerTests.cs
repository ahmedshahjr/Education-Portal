using EducationPortal.Application.Common.Behaviours;
using EducationPortal.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Common.Behaviours;

public sealed record SampleRequest(string Title);

/// <summary>
/// The pipeline logs request names and user ids only, never the request (SEC-LOG-02, SEC-PII-02).
/// </summary>
public class RequestLoggerTests
{
    private const string PersonalData = "Jane Example, born 2015-04-01";

    private CapturingLogger<SampleRequest> _logger = null!;
    private Mock<IUser> _user = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new CapturingLogger<SampleRequest>();
        _user = new Mock<IUser>();
    }

    [Test]
    public async Task ShouldLogTheRequestNameAndUserId()
    {
        var userId = Guid.NewGuid().ToString();
        _user.Setup(x => x.Id).Returns(userId);
        var behaviour = new LoggingBehaviour<SampleRequest>(_logger, _user.Object);

        await behaviour.Process(new SampleRequest(PersonalData), CancellationToken.None);

        var entry = _logger.Entries.ShouldHaveSingleItem();
        entry.Values["RequestName"].ShouldBe(nameof(SampleRequest));
        entry.Values["UserId"].ShouldBe(userId);
        ShouldNotContainTheRequest(entry);
    }

    [Test]
    public async Task ShouldLogAnEmptyUserIdForAnonymousRequests()
    {
        var behaviour = new LoggingBehaviour<SampleRequest>(_logger, _user.Object);

        await behaviour.Process(new SampleRequest(PersonalData), CancellationToken.None);

        var entry = _logger.Entries.ShouldHaveSingleItem();
        entry.Values["UserId"].ShouldBe(string.Empty);
        ShouldNotContainTheRequest(entry);
    }

    [Test]
    public async Task ShouldWarnAboutSlowRequestsWithoutLoggingTheRequest()
    {
        var behaviour = new PerformanceBehaviour<SampleRequest, int>(_logger, _user.Object);

        await behaviour.Handle(new SampleRequest(PersonalData), async _ =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(550));
            return 1;
        }, CancellationToken.None);

        var entry = _logger.Entries.ShouldHaveSingleItem();
        entry.Level.ShouldBe(LogLevel.Warning);
        entry.Values["RequestName"].ShouldBe(nameof(SampleRequest));
        ShouldNotContainTheRequest(entry);
    }

    [Test]
    public async Task ShouldLogUnhandledExceptionsWithoutTheRequestAndRethrow()
    {
        var behaviour = new UnhandledExceptionBehaviour<SampleRequest, int>(_logger);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            behaviour.Handle(new SampleRequest(PersonalData), _ => throw new InvalidOperationException("boom"), CancellationToken.None));

        var entry = _logger.Entries.ShouldHaveSingleItem();
        entry.Level.ShouldBe(LogLevel.Error);
        entry.Values["RequestName"].ShouldBe(nameof(SampleRequest));
        ShouldNotContainTheRequest(entry);
    }

    private static void ShouldNotContainTheRequest(CapturingLogger<SampleRequest>.LogEntry entry)
    {
        entry.Message.ShouldNotContain(PersonalData);
        entry.Values.Values.ShouldNotContain(value => value is SampleRequest);
        entry.Values.Values.OfType<string>().ShouldNotContain(value => value.Contains(PersonalData));
    }
}

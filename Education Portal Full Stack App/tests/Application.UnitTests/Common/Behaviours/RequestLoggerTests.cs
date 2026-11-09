using EducationPortal.Application.Common.Behaviours;
using EducationPortal.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EducationPortal.Application.UnitTests.Common.Behaviours;

public sealed record SampleRequest(string Title);

public class RequestLoggerTests
{
    private Mock<ILogger<SampleRequest>> _logger = null!;
    private Mock<IUser> _user = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<SampleRequest>>();
        _user = new Mock<IUser>();
        _identityService = new Mock<IIdentityService>();
    }

    [Test]
    public async Task ShouldCallGetUserNameAsyncOnceIfAuthenticated()
    {
        _user.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());

        var requestLogger = new LoggingBehaviour<SampleRequest>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new SampleRequest("title"), new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ShouldNotCallGetUserNameAsyncOnceIfUnauthenticated()
    {
        var requestLogger = new LoggingBehaviour<SampleRequest>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new SampleRequest("title"), new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Never);
    }
}

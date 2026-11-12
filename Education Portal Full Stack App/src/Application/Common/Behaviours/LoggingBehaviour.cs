using EducationPortal.Application.Common.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace EducationPortal.Application.Common.Behaviours;

/// <summary>
/// Logs which request runs and for which user id. The request itself and the user's name or email are
/// never logged, because requests carry personal data (SEC-LOG-02, SEC-PII-02).
/// </summary>
public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    private readonly ILogger _logger;
    private readonly IUser _user;

    public LoggingBehaviour(ILogger<TRequest> logger, IUser user)
    {
        _logger = logger;
        _user = user;
    }

    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("EducationPortal Request: {RequestName} {UserId}",
            typeof(TRequest).Name, _user.Id ?? string.Empty);

        return Task.CompletedTask;
    }
}

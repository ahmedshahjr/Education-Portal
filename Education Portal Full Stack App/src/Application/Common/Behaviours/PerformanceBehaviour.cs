using System.Diagnostics;
using EducationPortal.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace EducationPortal.Application.Common.Behaviours;

/// <summary>
/// Warns about requests that take longer than 500 ms. Logs the request name and user id only, never the
/// request itself (SEC-LOG-02, SEC-PII-02).
/// </summary>
public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly Stopwatch _timer;
    private readonly ILogger<TRequest> _logger;
    private readonly IUser _user;

    public PerformanceBehaviour(ILogger<TRequest> logger, IUser user)
    {
        _timer = new Stopwatch();

        _logger = logger;
        _user = user;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            _logger.LogWarning("EducationPortal Long Running Request: {RequestName} ({ElapsedMilliseconds} milliseconds) {UserId}",
                typeof(TRequest).Name, elapsedMilliseconds, _user.Id ?? string.Empty);
        }

        return response;
    }
}

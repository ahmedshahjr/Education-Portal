using Microsoft.Extensions.Logging;

namespace EducationPortal.Application.Common.Behaviours;

/// <summary>
/// Logs unhandled exceptions with the request name, then rethrows. The request itself is not logged,
/// because it can carry personal data (SEC-LOG-02, SEC-PII-02).
/// </summary>
public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger;

    public UnhandledExceptionBehaviour(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EducationPortal Request: Unhandled Exception for Request {RequestName}", typeof(TRequest).Name);

            throw;
        }
    }
}

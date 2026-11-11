using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Secrets;

namespace EducationPortal.Infrastructure.Secrets;

/// <summary>
/// Runs one SDK call and turns any failure into a <see cref="SecretProviderException"/> that names the key but never
/// the value. Cancellation by the caller passes through unchanged.
/// </summary>
internal static class SecretCall
{
    public static async Task<T> RunAsync<T>(
        string operation,
        SecretKey key,
        Func<Task<T>> call,
        Func<Exception, bool> isTransient,
        CancellationToken cancellationToken)
    {
        try
        {
            return await call();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (SecretProviderException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SecretProviderException(operation, key, isTransient(ex), ex);
        }
    }

    public static Task RunAsync(
        string operation,
        SecretKey key,
        Func<Task> call,
        Func<Exception, bool> isTransient,
        CancellationToken cancellationToken) =>
        RunAsync(operation, key, async () =>
        {
            await call();
            return true;
        }, isTransient, cancellationToken);

    /// <summary>Timeouts, throttling and server errors are worth retrying; client errors are not.</summary>
    public static bool IsTransientStatus(int status) => status is 0 or 408 or 429 or >= 500;
}

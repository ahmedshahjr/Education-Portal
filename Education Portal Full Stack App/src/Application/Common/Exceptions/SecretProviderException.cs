using EducationPortal.Application.Common.Secrets;

namespace EducationPortal.Application.Common.Exceptions;

/// <summary>
/// A secret store call failed. The message names the operation and the logical key, never the value
/// (SEC-SECRET-01, SEC-LOG-02).
/// </summary>
public sealed class SecretProviderException : Exception
{
    public SecretProviderException(string operation, SecretKey key, bool isTransient, Exception? innerException = null)
        : base($"Secret store operation '{operation}' failed for '{key.Path}'.", innerException)
    {
        Operation = operation;
        Key = key;
        IsTransient = isTransient;
    }

    public string Operation { get; }

    public SecretKey Key { get; }

    /// <summary>True when the call may succeed if retried (timeouts, throttling, server errors).</summary>
    public bool IsTransient { get; }
}

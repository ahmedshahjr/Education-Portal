using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Secrets;
using Microsoft.Extensions.Logging;

namespace EducationPortal.Infrastructure.IntegrationTests.Secrets;

/// <summary>A secret store in memory that counts calls.</summary>
internal sealed class InMemorySecretProvider : ISecretProvider
{
    public Dictionary<string, string> Values { get; } = [];

    public int Gets { get; private set; }

    public Task<string?> GetSecretAsync(SecretKey key, CancellationToken cancellationToken)
    {
        Gets++;
        return Task.FromResult(Values.TryGetValue(key.Path, out var value) ? value : null);
    }

    public Task SetSecretAsync(SecretKey key, string value, CancellationToken cancellationToken)
    {
        Values[key.Path] = value;
        return Task.CompletedTask;
    }

    public Task DeleteSecretAsync(SecretKey key, CancellationToken cancellationToken)
    {
        Values.Remove(key.Path);
        return Task.CompletedTask;
    }
}

/// <summary>Fails a set number of times with a <see cref="SecretProviderException"/>, then answers.</summary>
internal sealed class FlakySecretProvider(int failures, bool transient) : ISecretProvider
{
    public int Calls { get; private set; }

    public Task<string?> GetSecretAsync(SecretKey key, CancellationToken cancellationToken)
    {
        Fail("get", key);
        return Task.FromResult<string?>("value");
    }

    public Task SetSecretAsync(SecretKey key, string value, CancellationToken cancellationToken)
    {
        Fail("set", key);
        return Task.CompletedTask;
    }

    public Task DeleteSecretAsync(SecretKey key, CancellationToken cancellationToken)
    {
        Fail("delete", key);
        return Task.CompletedTask;
    }

    private void Fail(string operation, SecretKey key)
    {
        if (++Calls <= failures)
        {
            throw new SecretProviderException(operation, key, transient);
        }
    }
}

internal sealed class FakeCurrentTenant(int? schoolId) : ICurrentTenant
{
    public int? SchoolId { get; } = schoolId;

    public bool IsMaster => false;
}

/// <summary>Records every rendered log message and its exception.</summary>
internal sealed class CapturingLogger : ILogger
{
    public List<string> Messages { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Messages.Add(formatter(state, exception));
        if (exception is not null)
        {
            Messages.Add(exception.ToString());
        }
    }
}

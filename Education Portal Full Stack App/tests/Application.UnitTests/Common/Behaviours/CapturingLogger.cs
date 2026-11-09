using Microsoft.Extensions.Logging;

namespace EducationPortal.Application.UnitTests.Common.Behaviours;

/// <summary>
/// Records every log entry with its rendered message and its structured values, so tests can check
/// exactly what reaches the logs.
/// </summary>
public sealed class CapturingLogger<T> : ILogger<T>
{
    public List<LogEntry> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var values = state is IEnumerable<KeyValuePair<string, object?>> pairs
            ? pairs.ToDictionary(p => p.Key, p => p.Value)
            : [];

        Entries.Add(new LogEntry(logLevel, formatter(state, exception), values, exception));
    }

    public sealed record LogEntry(LogLevel Level, string Message, IReadOnlyDictionary<string, object?> Values, Exception? Exception);
}

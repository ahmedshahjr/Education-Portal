using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Secrets;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace EducationPortal.Infrastructure.Secrets;

/// <summary>
/// Retries transient store failures (timeouts, throttling, server errors) with exponential backoff and jitter, and
/// limits each attempt to <see cref="AttemptTimeout"/> (ADR-006). Retries are logged with the key, never the value
/// (SEC-LOG-02).
/// </summary>
public sealed class ResilientSecretProvider(ISecretProvider inner, ResiliencePipeline pipeline) : ISecretProvider
{
    public const int MaxRetryAttempts = 3;

    public static readonly TimeSpan BaseDelay = TimeSpan.FromMilliseconds(200);

    public static readonly TimeSpan AttemptTimeout = TimeSpan.FromSeconds(10);

    public ISecretProvider Inner => inner;

    public static ResiliencePipeline CreatePipeline(ILogger logger, TimeSpan? baseDelay = null)
    {
        ArgumentNullException.ThrowIfNull(logger);

        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<SecretProviderException>(ex => ex.IsTransient)
                    .Handle<TimeoutRejectedException>(),
                MaxRetryAttempts = MaxRetryAttempts,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = baseDelay ?? BaseDelay,
                OnRetry = args =>
                {
                    var key = (args.Outcome.Exception as SecretProviderException)?.Key.Path ?? "(timeout)";
                    logger.LogWarning(
                        "Secret store call for {SecretKey} failed; retry {Attempt} of {MaxAttempts} in {Delay}.",
                        key, args.AttemptNumber + 1, MaxRetryAttempts, args.RetryDelay);
                    return ValueTask.CompletedTask;
                },
            })
            .AddTimeout(AttemptTimeout)
            .Build();
    }

    public Task<string?> GetSecretAsync(SecretKey key, CancellationToken cancellationToken) =>
        pipeline.ExecuteAsync(
            static async (state, ct) => await state.inner.GetSecretAsync(state.key, ct),
            (inner, key),
            cancellationToken).AsTask();

    public Task SetSecretAsync(SecretKey key, string value, CancellationToken cancellationToken) =>
        pipeline.ExecuteAsync(
            static async (state, ct) => await state.inner.SetSecretAsync(state.key, state.value, ct),
            (inner, key, value),
            cancellationToken).AsTask();

    public Task DeleteSecretAsync(SecretKey key, CancellationToken cancellationToken) =>
        pipeline.ExecuteAsync(
            static async (state, ct) => await state.inner.DeleteSecretAsync(state.key, ct),
            (inner, key),
            cancellationToken).AsTask();
}

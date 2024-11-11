using Polly;
using Polly.Retry;

namespace Nounbase.Core.Utilities
{
    public static class Resiliency
    {
        public static ResiliencePipeline<T> CreateRetryPipeline<T>(int maxRetries = 3, int delaySeconds = 3, int timeoutSeconds = 15) =>
            new ResiliencePipelineBuilder<T>()
            .AddRetry(new RetryStrategyOptions<T>
            {
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                MaxRetryAttempts = maxRetries,
                Delay = TimeSpan.FromSeconds(delaySeconds),
            })
            .AddTimeout(TimeSpan.FromSeconds(timeoutSeconds))
            .Build();
    }
}

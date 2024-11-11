using Microsoft.Extensions.Logging;
using Nounbase.Services.SqlServer.Constants;
using static System.Environment;

namespace Nounbase.Services.SqlServer.Readers
{
    public abstract class BaseSqlServerDbReader
    {
        private readonly static SemaphoreSlim semaphore = CreateThrottle();

        static SemaphoreSlim CreateThrottle()
        {
            var throttle = 20;
            var envThrottle = GetEnvironmentVariable(NounbaseEnv.DbThrottle);

            if (!string.IsNullOrWhiteSpace(envThrottle))
            {
                if (!int.TryParse(envThrottle, out throttle) || (throttle <= 0))
                {
                    throw new InvalidOperationException($"[{NounbaseEnv.DbThrottle}] must be a valid integer > 0.");
                }
            }

            return new SemaphoreSlim(throttle);
        }

        protected SemaphoreSlim Semaphore { get; } = semaphore;
    }
}

using Polly;

namespace Confluence.Cli
{
    public static class Polices
    {
        public static int lastRetryDelayMillis = 5000;
        public static int maxRetryDelayMillis = 30000;
        public static double[] jitterMultiplierRange = { 0.7, 1.3 };

        public static IAsyncPolicy<HttpResponseMessage> RetryHonouringRetryAfter =
            Policy.HandleResult<HttpResponseMessage>(r => r?.Headers?.RetryAfter != null || r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryForeverAsync<HttpResponseMessage>(sleepDurationProvider: (times, response, ctx) =>
            {
                var timeToWait = response.Result.Headers.RetryAfter?.Delta ?? TimeSpan.FromMilliseconds(500);
                timeToWait = +new TimeSpan(0, 0, 0, 0, ThreadSafeRandom.Instance.Next(700, 1300));
                return timeToWait;
            },
            async (result, timeSpan, retryCount, context) =>
            {
                Spectre.Console.AnsiConsole.WriteLine($"RATE LIMITED retying...");
                await Task.CompletedTask;
            });

        public static IAsyncPolicy<HttpResponseMessage> RetryAfterError =
            Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            .WaitAndRetryForeverAsync<HttpResponseMessage>(sleepDurationProvider: (times, response, ctx) =>
            {
                int retryDelayMillis = lastRetryDelayMillis;

                retryDelayMillis += (int)(retryDelayMillis * (ThreadSafeRandom.Instance.NextDouble() * (jitterMultiplierRange[1] - jitterMultiplierRange[0]) + jitterMultiplierRange[0]));

                var timeToWait = TimeSpan.FromMilliseconds(retryDelayMillis);
                return timeToWait;
            },
             async (result, timeSpan, retryCount, context) =>
            {
                Spectre.Console.AnsiConsole.WriteLine($"Issue with URL: {result.Result?.RequestMessage?.RequestUri} retrying...");
                await Task.CompletedTask;
            });

        private class ThreadSafeRandom
        {
            private static readonly Random _global = new Random();

            private static readonly ThreadLocal<Random> _local = new ThreadLocal<Random>(() =>
            {
                int seed;
                lock (_global)
                {
                    seed = _global.Next();
                }
                return new Random(seed);
            });

            public static Random Instance => _local.Value;
        }
    }
}
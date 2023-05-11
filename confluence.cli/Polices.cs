using System.Net.Http.Headers;
using Polly;

namespace Confluence.Cli
{
    public static class Polices
    {
        public static IAsyncPolicy<HttpResponseMessage> RetryHonouringRetryAfter =
            Policy.HandleResult<HttpResponseMessage>(r => r?.Headers?.RetryAfter != null || r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryForeverAsync<HttpResponseMessage>(sleepDurationProvider: (times, response, ctx) =>
            {
                var timeToWait = response.Result.Headers.RetryAfter?.Delta ?? TimeSpan.FromMilliseconds(500);
                timeToWait =+ new TimeSpan(0, 0, 0, 0, ThreadSafeRandom.Instance.Next(700, 1300));
                return timeToWait;
            },
            async (result, timeSpan, retryCount, context) => await Console.Out.WriteLineAsync($"RATE LIMITED: TS: {timeSpan} retryCount: {retryCount}"));

        public static IAsyncPolicy<HttpResponseMessage> RetryAfterError =
            Policy.HandleResult<HttpResponseMessage>(r => r.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            .WaitAndRetryForeverAsync<HttpResponseMessage>(sleepDurationProvider: (times, response, ctx) =>
            {
                var timeToWait = response.Result.Headers.RetryAfter?.Delta ?? TimeSpan.FromMilliseconds(500);
                timeToWait =+ new TimeSpan(0, 0, 0, 0, ThreadSafeRandom.Instance.Next(700, 1300));
                return timeToWait;
            },
            async (result, timeSpan, retryCount, context) => await Console.Out.WriteLineAsync($"500 ERROR: TS: {timeSpan} retryCount: {retryCount}"));

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
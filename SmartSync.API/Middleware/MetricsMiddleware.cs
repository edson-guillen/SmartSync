using System.Diagnostics;

namespace SmartSync.API.Middleware
{
    public class MetricsMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;
        private static readonly Dictionary<string, int> _requestCounts = new();
        private static readonly Dictionary<string, long> _requestTimes = new();

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;
            var timer = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                timer.Stop();

                lock (_requestCounts)
                {
                    if (!_requestCounts.ContainsKey(path))
                    {
                        _requestCounts[path] = 0;
                        _requestTimes[path] = 0;
                    }

                    _requestCounts[path]++;
                    _requestTimes[path] += timer.ElapsedMilliseconds;
                }
            }
        }
    }

    public static class MetricsMiddlewareExtensions
    {
        public static IApplicationBuilder UseMetrics(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MetricsMiddleware>();
        }
    }
}
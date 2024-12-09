using System.Collections.Concurrent;

namespace Fetch.Api.Middleware
{
    public class UsageMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, EndpointStats> _stats = new();

        public UsageMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var startTime = DateTime.UtcNow;

            await _next(context);

            var elapsed = DateTime.UtcNow - startTime;
            var endpoint = context.Request.Path.ToString();
            var method = context.Request.Method;
            var statusCode = context.Response.StatusCode;

            var key = $"{method} {endpoint}";
            var stats = _stats.GetOrAdd(key, new EndpointStats());
            stats.Count++;
            stats.TotalResponseTime += elapsed.TotalMilliseconds;

            Console.WriteLine($"[{DateTime.UtcNow}] {method} {endpoint} responded {statusCode} in {elapsed.TotalMilliseconds} ms");
        }

        public static IReadOnlyDictionary<string, EndpointStats> GetStats() => _stats;
    }

    public class EndpointStats
    {
        public int Count { get; set; } = 0; // Number of requests for this endpoint
        public double TotalResponseTime { get; set; } = 0; // Total time taken for requests to this endpoint

        public double AverageResponseTime => Count > 0 ? TotalResponseTime / Count : 0; // Calculate average response time
    }
}

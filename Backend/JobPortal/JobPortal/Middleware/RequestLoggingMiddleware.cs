using System.Diagnostics;

namespace JobPortal.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var method = context.Request.Method;
                var path = context.Request.Path;
                var ip = context.Connection.RemoteIpAddress?.ToString();

                _logger.LogInformation("Received HTTP {Method} request to {Path} from {IP}",
                    method, path, ip);

                await _next(context); // Call the next middleware

                stopwatch.Stop();
                var statusCode = context.Response.StatusCode;

                if (statusCode >= 200 && statusCode < 300)
                {
                    _logger.LogInformation("HTTP {Method} {Path} responded with {StatusCode} in {Elapsed}ms",
                        method, path, statusCode, stopwatch.ElapsedMilliseconds);
                }
                else if (statusCode >= 400 && statusCode < 500)
                {
                    _logger.LogWarning("HTTP {Method} {Path} responded with {StatusCode} in {Elapsed}ms",
                        method, path, statusCode, stopwatch.ElapsedMilliseconds);
                }
                else if (statusCode >= 500)
                {
                    _logger.LogError("HTTP {Method} {Path} responded with {StatusCode} in {Elapsed}ms",
                        method, path, statusCode, stopwatch.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An unhandled exception occurred while processing the request.");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal server error");
            }
        }
    }
}

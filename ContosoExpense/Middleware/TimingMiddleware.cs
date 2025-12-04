using ContosoExpense.Services;
using System.Diagnostics;
using System.Security.Claims;

namespace ContosoExpense.Middleware;

/// <summary>
/// Middleware for request timing and metrics collection.
/// </summary>
public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestTimingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IMetricsService metricsService)
    {
        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        var path = context.Request.Path.Value ?? "/";
        var method = context.Request.Method;
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        metricsService.RecordRequest(path, method, stopwatch.ElapsedMilliseconds, userId);
    }
}

/// <summary>
/// Middleware for simulating latency and failures (for resilience testing).
/// </summary>
public class LatencySimulationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Random _random = new();

    public LatencySimulationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ISystemSettingsService settingsService)
    {
        var settings = settingsService.GetSettings();

        if (settings.SimulateLatency)
        {
            // Only simulate latency for POST/PUT/DELETE operations
            if (context.Request.Method != "GET")
            {
                await Task.Delay(settings.SimulatedLatencyMs);
            }

            // Simulate random failures
            if (settings.SimulatedFailureRate > 0 && _random.NextDouble() < settings.SimulatedFailureRate)
            {
                context.Response.StatusCode = 503;
                await context.Response.WriteAsJsonAsync(new { error = "Simulated service unavailable" });
                return;
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension methods for middleware registration.
/// </summary>
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestTiming(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestTimingMiddleware>();
    }

    public static IApplicationBuilder UseLatencySimulation(this IApplicationBuilder app)
    {
        return app.UseMiddleware<LatencySimulationMiddleware>();
    }
}

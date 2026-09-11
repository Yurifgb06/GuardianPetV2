using System.Diagnostics;
using GuardianPet.Observability;
using Microsoft.AspNetCore.Routing;

namespace GuardianPet.Middlewares;

public sealed class MetricsMiddleware
{
    private readonly RequestDelegate _next;

    public MetricsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var started = Stopwatch.GetTimestamp();
        var unhandledException = false;
        try
        {
            await _next(context);
        }
        catch
        {
            unhandledException = true;
            throw;
        }
        finally
        {
            var duration = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            var statusCode = unhandledException && !context.Response.HasStarted
                ? StatusCodes.Status500InternalServerError
                : context.Response.StatusCode;
            var pattern = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText;
            var route = pattern is null ? "unmatched" : "/" + pattern.TrimStart('/');
            // Unknown verbs and unmatched URLs must not create unbounded metric series.
            var method = context.Request.Method switch
            {
                "GET" or "POST" or "PUT" or "DELETE" or "PATCH" or "HEAD"
                    or "OPTIONS" or "TRACE" or "CONNECT" => context.Request.Method,
                _ => "OTHER"
            };
            var tags = new TagList
            {
                { "http.request.method", method },
                { "http.response.status_code", statusCode },
                { "http.route", route }
            };

            GuardianPetTelemetry.RequestDuration.Record(duration, tags);
            if (statusCode is >= 400 and <= 599)
            {
                GuardianPetTelemetry.HttpErrors.Add(1, tags);
            }
        }
    }
}

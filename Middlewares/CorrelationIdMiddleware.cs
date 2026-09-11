using Serilog.Context;

namespace GuardianPet.Middlewares;

public sealed class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-ID";
    private const int MaxLength = 128;
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var values = context.Request.Headers[HeaderName];
        var suppliedId = values.Count == 1 ? values[0] : null;
        var correlationId = IsValid(suppliedId) ? suppliedId! : Guid.NewGuid().ToString("D");

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    private static bool IsValid(string? value)
    {
        return value is { Length: > 0 and <= MaxLength }
            && value.All(character => char.IsAsciiLetterOrDigit(character)
                || character is '-' or '_' or '.');
    }
}

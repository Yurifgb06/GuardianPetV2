using System.Net;
using System.Text.Json;
using GuardianPet.Exceptions;

namespace GuardianPet.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ApiException ex)
            {
                _logger.LogWarning("Request rejected with status {StatusCode}", ex.StatusCode);
                await HandleExceptionAsync(context, ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                // Exception messages and data can contain credentials or user input.
                _logger.LogError("Unexpected exception {ExceptionType}; stack trace: {StackTrace}",
                    ex.GetType().FullName, ex.StackTrace);
                await HandleExceptionAsync(
                    context,
                    (int)HttpStatusCode.InternalServerError,
                    "Erro interno do servidor."
                );
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, int statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web));

            await context.Response.WriteAsync(json);
        }
    }
}

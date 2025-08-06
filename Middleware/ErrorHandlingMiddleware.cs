using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SamaraCloudsApi.Models; // Pastikan ApiErrorResponse di sini

namespace SamaraCloudsApi.Middleware
{
    /// <summary>
    /// Middleware for handling exceptions and returning standardized error responses.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex, _env);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception, IHostEnvironment env)
        {
            context.Response.ContentType = "application/json";

            int statusCode = (int)HttpStatusCode.InternalServerError;
            string code = "INTERNAL_SERVER_ERROR";
            string message = "An error occurred while processing your request.";
            object errors = exception.Message;

            // Tentukan status code dan code string sesuai exception type
            switch (exception)
            {
                case ValidationException ve:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    code = "BAD_REQUEST";
                    message = "Validation failed.";
                    errors = ve.Message;
                    break;
                case ArgumentException ae:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    code = "BAD_REQUEST";
                    message = "Invalid request parameters.";
                    errors = ae.Message;
                    break;
                case UnauthorizedAccessException ue:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    code = "UNAUTHORIZED";
                    message = "Access denied.";
                    errors = ue.Message;
                    break;
                case InvalidOperationException ioe:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    code = "BAD_REQUEST";
                    message = "Invalid operation.";
                    errors = ioe.Message;
                    break;
                // Tambah case exception lain jika perlu
            }

            context.Response.StatusCode = statusCode;

            var errorResponse = new ApiErrorResponse
            {
                Status = statusCode,
                Code = code,
                Message = message,
                Errors = errors
            };

            // Jika development, tambahkan detail exception (stacktrace, inner exception)
            if (env.IsDevelopment())
            {
                var detailedErrors = new
                {
                    errorResponse.Status,
                    errorResponse.Code,
                    errorResponse.Message,
                    Errors = errors,
                    Exception = exception.ToString(),
                    InnerException = exception.InnerException?.Message
                };

                var detailedJson = JsonSerializer.Serialize(detailedErrors, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                await context.Response.WriteAsync(detailedJson);
                return;
            }

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}

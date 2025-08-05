using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace SamaraCloudsApi.Middleware
{
    /// <summary>
    /// Middleware for handling exceptions and returning standardized error responses.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IWebHostEnvironment env)
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
                await HandleExceptionAsync(context, ex, _env, _logger);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception, IWebHostEnvironment env, ILogger logger)
        {
            context.Response.ContentType = "application/json";
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string message = "An error occurred while processing your request.";
            string error = exception.Message;

            // Default structure
            var response = new ErrorResponse
            {
                Success = false,
                Message = message,
                Error = error,
                Timestamp = DateTime.UtcNow
            };

            // Tambahkan pengecekan tipe exception
            switch (exception)
            {
                case ValidationException ve:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Validation failed.";
                    response.Error = ve.Message;
                    break;
                case ArgumentException ae:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Invalid request parameters.";
                    response.Error = ae.Message;
                    break;
                case UnauthorizedAccessException ue:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = "Access denied.";
                    response.Error = ue.Message;
                    break;
                case InvalidOperationException ioe:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Invalid operation.";
                    response.Error = ioe.Message;
                    break;
                // TODO: Tambahkan custom exception lain di sini
            }

            // Untuk development, tampilkan inner exception dan stacktrace
            if (env.IsDevelopment())
            {
                response.Details = exception.ToString();
                if (exception.InnerException != null)
                {
                    response.InnerError = exception.InnerException.Message;
                }
            }

            context.Response.StatusCode = statusCode;

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        /// <summary>
        /// Standard error response format.
        /// </summary>
        public class ErrorResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = default!;
            public string Error { get; set; } = default!;
            public DateTime Timestamp { get; set; }
            public string? Details { get; set; }    // For development
            public string? InnerError { get; set; } // For development
        }
    }
}

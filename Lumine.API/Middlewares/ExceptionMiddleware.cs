using Domain.Base;
using FirebaseAdmin.Auth;
using System.Text.Json;

namespace Lumine.API.Middlewares
{
    /// <summary>  
    /// Middleware for handling exceptions globally in the application.  
    /// Logs exceptions and returns structured error responses.  
    /// </summary>  
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        /// <summary>  
        /// Initializes a new instance of the <see cref="ExceptionMiddleware"/> class.  
        /// </summary>  
        /// <param name="next">The next middleware in the pipeline.</param>  
        /// <param name="logger">The logger instance for logging exceptions.</param>  
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>  
        /// Invokes the middleware to handle exceptions during the request pipeline execution.  
        /// </summary>  
        /// <param name="context">The HTTP context of the current request.</param>  
        /// <returns>A task representing the asynchronous operation.</returns>  
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (BaseException.CoreException ex)
            {
                await HandleExceptionAsync(context, ex.StatusCode, new
                {
                    errorCode = ex.Code,
                    errorMessage = ex.Message,
                    additionalData = ex.AdditionalData
                }, ex);
            }
            catch (BaseException.BadRequestException ex)
            {
                await HandleExceptionAsync(context, ex.StatusCode, new
                {
                    errorCode = ex.ErrorDetail?.ErrorCode,
                    errorMessage = ex.ErrorDetail?.ErrorMessage
                }, ex);
            }
            catch (BaseException.NotFoundException ex)
            {
                await HandleExceptionAsync(context, ex.StatusCode, new
                {
                    errorCode = ex.ErrorDetail?.ErrorCode,
                    errorMessage = ex.ErrorDetail?.ErrorMessage
                }, ex);
            }
            catch (BaseException.UnauthorizedException ex)
            {
                await HandleExceptionAsync(context, ex.StatusCode, new
                {
                    errorCode = ex.ErrorDetail?.ErrorCode,
                    errorMessage = ex.ErrorDetail?.ErrorMessage
                }, ex);
            }
            catch (BaseException.ValidationException ex)
            {
                await HandleExceptionAsync(context, ex.StatusCode, new
                {
                    errorCode = ex.ErrorDetail?.ErrorCode,
                    errorMessage = ex.ErrorDetail?.ErrorMessage
                }, ex);
            }
            catch (FirebaseAuthException ex)
            {
                var message = ex.AuthErrorCode == AuthErrorCode.ExpiredIdToken
                    ? "Firebase token has expired."
                    : "Invalid Firebase token.";

                await HandleExceptionAsync(context, StatusCodes.Status401Unauthorized, new
                {
                    errorCode = "invalid_firebase_token",
                    errorMessage = message
                }, ex);
            }

            catch (Exception ex)
            {
                await HandleExceptionAsync(context, StatusCodes.Status500InternalServerError, new
                {
                    errorCode = "INTERNAL_SERVER_ERROR",
                    errorMessage = "An unexpected error occurred. Please try again later."
                }, ex);
            }
        }

        /// <summary>  
        /// Handles exceptions by logging and returning a structured response.  
        /// </summary>  
        /// <param name="context">HTTP context of the request.</param>  
        /// <param name="statusCode">HTTP status code to return.</param>  
        /// <param name="result">The structured error response.</param>  
        /// <param name="ex">The exception that occurred.</param>  
        /// <returns>A task representing the exception handling operation.</returns>  
        private async Task HandleExceptionAsync(HttpContext context, int statusCode, object result, Exception ex)
        {
            _logger.LogError(ex, $"Exception occurred: {ex.Message}");

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var responseJson = JsonSerializer.Serialize(result, jsonOptions);

            await context.Response.WriteAsync(responseJson);
        }
    }

    /// <summary>  
    /// Provides extension methods for adding the <see cref="ExceptionMiddleware"/> to the application pipeline.  
    /// </summary>  
    public static class ExceptionMiddlewareExtensions
    {
        /// <summary>  
        /// Adds the <see cref="ExceptionMiddleware"/> to the application's request pipeline.  
        /// </summary>  
        /// <param name="app">The application builder.</param>  
        /// <returns>The application builder with the middleware added.</returns>  
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}

using System.Data.Common;
using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Capitec.FraudEngine.API.Infrastructure
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        private const string ConflictTitle = "Conflict";
        private const string BadRequestTitle = "Bad Request";
        private const string UnauthorizedTitle = "Unauthorized";
        private const string ServiceUnavailableTitle = "Service Unavailable";
        private const string DatabaseErrorTitle = "Database Error";
        private const string InternalServerErrorTitle = "Internal Server Error";

        private const string UnauthorizedDetail = "You are not authorized to perform this action.";
        private const string BackendTimeoutDetail = "A transient backend timeout occurred. Please try again.";
        private const string TransientDatabaseDetail = "A transient database error occurred. Please try again.";
        private const string DatabaseErrorDetail = "A database error occurred while processing the request.";
        private const string UnexpectedErrorDetail = "An unexpected error occurred while processing the request.";

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            // send to validation handler to process FluentValidation exceptions.
            if (exception is ValidationException)
            {
                return false;
            }

            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
            var correlationIdHeader = httpContext.Request.Headers["X-Correlation-ID"].ToString();
            var correlationId = string.IsNullOrWhiteSpace(correlationIdHeader) ? traceId : correlationIdHeader;

            var (status, title, detail) = MapException(httpContext, exception);

            logger.LogError(
                exception,
                "Unhandled exception mapped to problem details. Status: {Status}, TraceId: {TraceId}, CorrelationId: {CorrelationId}",
                status,
                traceId,
                correlationId);

            var problemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions["traceId"] = traceId;
            problemDetails.Extensions["correlationId"] = correlationId;

            httpContext.Response.StatusCode = status;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private static (int Status, string Title, string Detail) MapException(HttpContext httpContext, Exception exception)
        {
            return exception switch
            {
                InvalidOperationException invalidOperationException => (
                    StatusCodes.Status409Conflict,
                    ConflictTitle,
                    invalidOperationException.Message),

                ArgumentException argumentException => (
                    StatusCodes.Status400BadRequest,
                    BadRequestTitle,
                    argumentException.Message),

                UnauthorizedAccessException => (
                    httpContext.User?.Identity?.IsAuthenticated == true
                        ? StatusCodes.Status403Forbidden
                        : StatusCodes.Status401Unauthorized,
                    UnauthorizedTitle,
                    UnauthorizedDetail),

                TimeoutException => (
                    StatusCodes.Status503ServiceUnavailable,
                    ServiceUnavailableTitle,
                    BackendTimeoutDetail),

                DbUpdateException dbUpdateException when dbUpdateException.InnerException is DbException dbException && dbException.IsTransient => (
                    StatusCodes.Status503ServiceUnavailable,
                    ServiceUnavailableTitle,
                    TransientDatabaseDetail),

                DbUpdateException => (
                    StatusCodes.Status500InternalServerError,
                    DatabaseErrorTitle,
                    DatabaseErrorDetail),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    InternalServerErrorTitle,
                    UnexpectedErrorDetail)
            };
        }
    }
}

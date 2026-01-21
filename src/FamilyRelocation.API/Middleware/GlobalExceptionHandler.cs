using System.Net;
using FamilyRelocation.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace FamilyRelocation.API.Middleware;

/// <summary>
/// Global exception handler that converts exceptions to appropriate HTTP responses.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, response) = MapException(exception);

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning(exception, "A handled exception occurred: {Message}", exception.Message);
        }

        httpContext.Response.StatusCode = (int)statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    private static (HttpStatusCode StatusCode, object Response) MapException(Exception exception)
    {
        return exception switch
        {
            ValidationException ve => (
                HttpStatusCode.BadRequest,
                new ErrorResponse(
                    "Validation Failed",
                    ve.Errors.Count > 0 ? ve.Errors : [ve.Message]
                )
            ),
            NotFoundException nf => (
                HttpStatusCode.NotFound,
                new ErrorResponse("Not Found", [nf.Message])
            ),
            DuplicateEmailException de => (
                HttpStatusCode.Conflict,
                new ErrorResponse("Conflict", [de.Message])
            ),
            ArgumentException ae => (
                HttpStatusCode.BadRequest,
                new ErrorResponse("Bad Request", [ae.Message])
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.Forbidden,
                new ErrorResponse("Forbidden", ["Access denied"])
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse("Internal Server Error", ["An unexpected error occurred. Please try again later."])
            )
        };
    }
}

/// <summary>
/// Standard error response format.
/// </summary>
public record ErrorResponse(string Title, IEnumerable<string> Errors);

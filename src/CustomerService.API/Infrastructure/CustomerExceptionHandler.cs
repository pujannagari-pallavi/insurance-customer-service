using CustomerService.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;

namespace CustomerService.API.Infrastructure;

public sealed class CustomerExceptionHandler(ILogger<CustomerExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var isKycInfrastructureFailure = exception is SocketException or HttpRequestException
            || exception.Message.StartsWith("KYC ", StringComparison.Ordinal);

        if (exception is not ValidationException and not NotFoundException and not UnauthorizedAccessException && !isKycInfrastructureFailure)
        {
            return false;
        }

        logger.LogWarning(exception, "Request failed with a handled application exception.");

        var problemDetails = new ProblemDetails
        {
            Status = exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                NotFoundException => StatusCodes.Status404NotFound,
                _ when isKycInfrastructureFailure => StatusCodes.Status503ServiceUnavailable,
                _ => StatusCodes.Status403Forbidden
            },
            Title = exception switch
            {
                ValidationException => "Validation failed.",
                NotFoundException => "Resource not found.",
                _ when isKycInfrastructureFailure => "KYC upload temporarily unavailable.",
                _ => "Access denied."
            },
            Detail = isKycInfrastructureFailure
                ? "KYC document scanning or secure storage is not available. Please try again later."
                : exception.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}

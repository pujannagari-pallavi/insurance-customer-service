using CustomerService.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.API.Infrastructure;

public sealed class CustomerExceptionHandler(ILogger<CustomerExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException and not NotFoundException and not UnauthorizedAccessException)
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
                _ => StatusCodes.Status403Forbidden
            },
            Title = exception switch
            {
                ValidationException => "Validation failed.",
                NotFoundException => "Resource not found.",
                _ => "Access denied."
            },
            Detail = exception.Message
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}

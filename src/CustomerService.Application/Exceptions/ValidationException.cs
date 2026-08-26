namespace CustomerService.Application.Exceptions;

public sealed class ValidationException(string message) : Exception(message);

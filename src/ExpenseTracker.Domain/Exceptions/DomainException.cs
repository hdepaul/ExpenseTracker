namespace ExpenseTracker.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.")
    {
    }
}

public class ValidationException : DomainException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string errorMessage)
        : base(errorMessage)
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        };
    }
}

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "Unauthorized access.")
        : base(message)
    {
    }
}

public class ForbiddenException : DomainException
{
    public ForbiddenException(string message = "Access forbidden.")
        : base(message)
    {
    }
}

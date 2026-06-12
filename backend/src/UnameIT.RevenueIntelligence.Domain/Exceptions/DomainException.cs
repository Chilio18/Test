namespace UnameIT.RevenueIntelligence.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception inner) : base(message, inner) { }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string entity, object key)
        : base($"{entity} with key '{key}' was not found.") { }
}

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "You do not have permission to perform this action.")
        : base(message) { }
}

public class TenantMismatchException : DomainException
{
    public TenantMismatchException()
        : base("The requested resource does not belong to the current tenant.") { }
}

public class ValidationException : DomainException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = new Dictionary<string, string[]>(errors);
    }
}

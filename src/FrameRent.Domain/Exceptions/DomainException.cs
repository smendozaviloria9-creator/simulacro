namespace FrameRent.Domain.Exceptions;

/// <summary>
/// Business rule violation. Mapped to HTTP 400 Bad Request.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}

/// <summary>
/// The requested resource does not exist. Mapped to HTTP 404 Not Found.
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message)
    {
    }
}

/// <summary>
/// The user is authenticated but not allowed to act on the resource. Mapped to HTTP 403 Forbidden.
/// </summary>
public class ForbiddenException : DomainException
{
    public ForbiddenException(string message) : base(message)
    {
    }
}

/// <summary>
/// Invalid credentials or locked account. Mapped to HTTP 401 Unauthorized.
/// </summary>
public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}

namespace CoherentMobile.Application.Exceptions;

/// <summary>
/// Exception for authentication failures
/// </summary>
public class AuthenticationException : Exception
{
    public string ErrorCode { get; }

    public AuthenticationException(string message, string errorCode = "AUTH_ERROR") 
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public AuthenticationException(string message, Exception innerException, string errorCode = "AUTH_ERROR") 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

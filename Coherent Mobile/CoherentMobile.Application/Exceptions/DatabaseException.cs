namespace CoherentMobile.Application.Exceptions;

/// <summary>
/// Exception for database operation failures
/// </summary>
public class DatabaseException : Exception
{
    public string ErrorCode { get; }

    public DatabaseException(string message, string errorCode = "DB_ERROR") 
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public DatabaseException(string message, Exception innerException, string errorCode = "DB_ERROR") 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

namespace CoherentMobile.Application.Exceptions;

/// <summary>
/// Exception for OTP related failures
/// </summary>
public class OTPException : Exception
{
    public string ErrorCode { get; }

    public OTPException(string message, string errorCode = "OTP_ERROR") 
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public OTPException(string message, Exception innerException, string errorCode = "OTP_ERROR") 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

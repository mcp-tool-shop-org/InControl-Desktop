namespace Volt.Core.Exceptions;

/// <summary>
/// Base exception for all Volt-specific errors.
/// </summary>
public class VoltException : Exception
{
    /// <summary>
    /// Error code for categorizing the exception.
    /// </summary>
    public string? ErrorCode { get; }

    public VoltException()
    {
    }

    public VoltException(string message) : base(message)
    {
    }

    public VoltException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    public VoltException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public VoltException(string message, string errorCode, Exception innerException) : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

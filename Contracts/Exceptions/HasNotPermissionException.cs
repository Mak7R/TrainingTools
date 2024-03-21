namespace Contracts.Exceptions;

public class HasNotPermissionException : Exception
{
    public HasNotPermissionException() {}
    public HasNotPermissionException(string message) : base(message){}
    public HasNotPermissionException(string message, Exception? innerException) : base(message, innerException) {}
}